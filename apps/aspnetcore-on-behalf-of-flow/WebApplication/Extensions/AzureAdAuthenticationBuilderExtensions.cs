using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Threading.Tasks;
using WebApplication.Helper;

namespace WebApplication.Extensions
{
    public static class AzureAdAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder)
            => builder.AddAzureAd(_ => { });

        public static AuthenticationBuilder AddAzureAd(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<OpenIdConnectOptions>, ConfigureAzureOptions>();
            builder.Services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, ConfigureCookieOptions>();
            builder.AddOpenIdConnect();
            builder.AddCookie();
            return builder;
        }

        public class ConfigureAzureOptions : IConfigureNamedOptions<OpenIdConnectOptions>
        {
            private readonly AzureAdOptions _azureOptions;
            private readonly TokenService _tokenService;

            public ConfigureAzureOptions(IOptions<AzureAdOptions> options, TokenService tokenService)
            {
                _azureOptions = options.Value;
                _tokenService = tokenService;
            }

            public void Configure(string name, OpenIdConnectOptions options)
            {
                // Set the application id
                options.ClientId = _azureOptions.ClientId;

                // the authority {Instance}/{Tenant}/v2.0
                // ASP.NET uses v1.0 by default
                options.Authority = $"{_azureOptions.Instance}{_azureOptions.TenantId}/v2.0";
                options.UseTokenLifetime = true;
                // the redirect Uri: constructed from host:port/signin-oidc
                options.CallbackPath = _azureOptions.CallbackPath;
                // we are in development environment, don't do this in production
                options.RequireHttpsMetadata = false;
                // set name of user name claim
                options.TokenValidationParameters.NameClaimType = "preferred_username";
                // We ask ASP.NET to request a Code and an Id Token
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                foreach (var scope in _azureOptions.ConsentScopes)
                    options.Scope.Add(scope);

                options.Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = context =>
                    {
                        // If your authentication logic is based on users then add your logic here
                        // If you want to add additional claims, this is the right place.
                        // e.g.:
                        // var objectId = context.Principal.GetObjectId();

                        // var db = context.HttpContext.RequestServices.GetRequiredService<AuthorizationDbContext>();
                        // bool isSuperAdmin = await db.SuperAdmins.AnyAsync(a => a.ObjectId == objectId);
                        // if (isSuperAdmin)
                        // {
                        //     //Add claim if they are
                        //     var claims = new List<Claim>
                        //     {
                        //         new Claim(ClaimTypes.Role, "superadmin")
                        //     };
                        //     var appIdentity = new ClaimsIdentity(claims);

                        //     context.Principal.AddIdentity(appIdentity);
                        // }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        context.Response.Redirect("/Home/Error");
                        context.HandleResponse(); // Suppress the exception
                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        var user = context.HttpContext.User;

                        context.ProtocolMessage.LoginHint = user.GetLoginHint();
                        context.ProtocolMessage.DomainHint = user.GetDomainHint();

                        _tokenService.RemoveAccount(user);
                        return Task.FromResult(true);
                    },
                    OnAuthorizationCodeReceived = async (context) =>
                    {
                        // As AcquireTokenByAuthorizationCode is asynchronous we want to tell ASP.NET core
                        // that we are handing the code even if it's not done yet, so that it does 
                        // not concurrently call the Token endpoint.
                        context.HandleCodeRedemption();

                        var code = context.ProtocolMessage.Code;
                        
                        var result = await _tokenService.GetAccessTokenByAuthorizationCodeAsync(context.Principal, code, _azureOptions.ApiScopes);

                        // Do not share the access token with ASP.NET Core otherwise ASP.NET will cache it
                        // and will not send the OAuth 2.0 request in case a further call to
                        // AcquireTokenByAuthorizationCode in the future for incremental consent 
                        // (getting a code requesting more scopes)
                        // Share the ID Token so that the identity of the user is known in the application (in 
                        // HttpContext.User)
                        context.HandleCodeRedemption(null, result.IdToken);
                    },
                    OnTokenValidated = (context) =>
                    {
                        return Task.CompletedTask;
                    }
                };
            }

            public void Configure(OpenIdConnectOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }

        public class ConfigureCookieOptions : IConfigureNamedOptions<CookieAuthenticationOptions>
        {
            private readonly TicketStoreService _ticketStore;

            public ConfigureCookieOptions(TicketStoreService service)
            {
                _ticketStore = service;
            }

            public void Configure(string name, CookieAuthenticationOptions options)
            {
                options.SessionStore = _ticketStore;
            }

            public void Configure(CookieAuthenticationOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}
