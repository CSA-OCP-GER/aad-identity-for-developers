using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IdentityModel.Tokens.Jwt;
using WebApi.Helper;

namespace WebApi.Extensions
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder)
            => builder.AddAzureAdBearer(_ => { });

        public static AuthenticationBuilder AddAzureAdBearer(this AuthenticationBuilder builder, Action<AzureAdOptions> configureOptions)
        {
            builder.Services.Configure(configureOptions);
            builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureAzureAdOptions>();
            builder.AddJwtBearer();
            return builder;
        }

        private class ConfigureAzureAdOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            private readonly AzureAdOptions _azureAdOptions;

            public ConfigureAzureAdOptions(IOptions<AzureAdOptions> options)
            {
                _azureAdOptions = options.Value;
            }

            public void Configure(string name, JwtBearerOptions options)
            {
                // the application id of Azure AD app
                options.Audience = _azureAdOptions.ClientId;
                // we use Azure AD v2.0 endpoint
                options.Authority = $"{_azureAdOptions.Instance}/{_azureAdOptions.TenantId}/v2.0";
                // as we have configured a custom identifier uri, we use it here to validate the adience
                options.TokenValidationParameters.ValidAudiences = new string[] { _azureAdOptions.ClientIdUri };
                // as we use a Azure AD tenent, we have to use tenant id here to validate the issuer
                options.TokenValidationParameters.ValidIssuer = $"https://sts.windows.net/{_azureAdOptions.TenantId}/";

                options.Events = new JwtBearerEvents
                {
                    // here the token was validated, we can use it to acquire a token for Microsoft Graph API
                    // and cache the issued token
                    OnTokenValidated = async context =>
                    {
                        var token = (JwtSecurityToken)context.SecurityToken;
                        var tokenService = context.HttpContext.RequestServices.GetRequiredService<TokenService>();
                        await tokenService.GetAccessTokenByJwtTokenAsync(context.Principal, token, _azureAdOptions.GraphScopes);
                    }
                };
            }

            public void Configure(JwtBearerOptions options)
            {
                Configure(Options.DefaultName, options);
            }
        }
    }
}
