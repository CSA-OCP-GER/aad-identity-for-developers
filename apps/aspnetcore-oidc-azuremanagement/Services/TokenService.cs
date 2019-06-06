using aspnetcore_oidc_azuremanagement.Extensions;
using aspnetcore_oidc_azuremanagement.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.Services
{
    public class TokenService
    {
        private readonly AzureAdOptions _azureAdOptions;
        private readonly string[] _scopes = new[] { "https://graph.microsoft.com/profile" };
        private readonly TokenCacheproviderFactory _tokenCacheProviderFactory;

        public TokenService(IOptions<AzureAdOptions> options, TokenCacheproviderFactory tokenCacheProviderFactory)
        {
            _azureAdOptions = options.Value;
            _tokenCacheProviderFactory = tokenCacheProviderFactory;
        }

        public async Task<AuthenticationResult> GetAccessTokenByAuthorizationCodeAsync(ClaimsPrincipal principal, string code)
        {
            var app = BuildApp(principal);
            var result = await app.AcquireTokenByAuthorizationCode(_scopes, code).ExecuteAsync().ConfigureAwait(false);
            var account = await app.GetAccountAsync(principal.GetMsalAccountId());
            return result;
        }

        public async Task<string> GetAccessTokenAsync(ClaimsPrincipal principal, string scope)
        {
            var app = BuildApp(principal);
            var account = await app.GetAccountAsync(principal.GetMsalAccountId());

            // guest??
            if (null == account)
            {
                var accounts = await app.GetAccountsAsync();
                account = accounts.FirstOrDefault(a => a.Username == principal.GetLoginHint());
            }

            var token = await app.AcquireTokenSilent(new string[] { scope }, account).ExecuteAsync().ConfigureAwait(false);
            return token.AccessToken;
        }

        public async Task<string> GetAccessTokenAsync(string scope)
        {
            var app = BuildApp();
            var token = await app.AcquireTokenForClient(new string[] { scope }).ExecuteAsync().ConfigureAwait(false);
            return token.AccessToken;
        }

        public async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, string scope)
        {
            var app = BuildApp(clientId, clientSecret);
            var token = await app.AcquireTokenForClient(new string[] { scope }).ExecuteAsync().ConfigureAwait(false);
            return token.AccessToken;
        }

        public void RemoveAccount(ClaimsPrincipal principal)
        {
            _tokenCacheProviderFactory.Create(principal).Clear();
        }

        private IConfidentialClientApplication BuildApp(ClaimsPrincipal principal)
        {
            var app = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                .WithClientSecret(_azureAdOptions.ClientSecret)
                // we only allow users from our tenant
                .WithAuthority(AzureCloudInstance.AzurePublic, Guid.Parse(_azureAdOptions.TenantId))
                // reply url
                .WithRedirectUri(_azureAdOptions.BaseUrl + _azureAdOptions.CallbackPath)
                .Build();

            _tokenCacheProviderFactory.Create(principal).Initialize(app.UserTokenCache);

            return app;
        }

        private IConfidentialClientApplication BuildApp()
        {
            var app = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                .WithClientSecret(_azureAdOptions.ClientSecret)
                .WithAuthority(AzureCloudInstance.AzurePublic, Guid.Parse(_azureAdOptions.TenantId))
                .Build();

            _tokenCacheProviderFactory.Create().Initialize(app.AppTokenCache);
            return app;
        }

        public IConfidentialClientApplication BuildApp(string clientId, string clientSecret)
        {
            var app = ConfidentialClientApplicationBuilder.Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(AzureCloudInstance.AzurePublic, Guid.Parse(_azureAdOptions.TenantId))
                .Build();

            _tokenCacheProviderFactory.Create(clientId).Initialize(app.AppTokenCache);
            return app;
        }
    }
}
