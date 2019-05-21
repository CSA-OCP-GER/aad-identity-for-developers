using aspnetcore_mvc_oauth2_code_grant.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace aspnetcore_mvc_oauth2_code_grant.Helper
{
    public class TokenService
    {
        private readonly AzureAdOptions _azureAdOptions;
        private readonly string[] _scopes = new[] { "https://graph.microsoft.com/User.Read" };
        private readonly UserTokenCacheProviderFactory _userTokenCacheProviderFactory;

        public TokenService(IOptions<AzureAdOptions> options, UserTokenCacheProviderFactory userTokenCacheProviderFactory)
        {
            _azureAdOptions = options.Value;
            _userTokenCacheProviderFactory = userTokenCacheProviderFactory;
        }

        public async Task<AuthenticationResult> GetAccessTokenByAuthorizationCodeAsync(ClaimsPrincipal principal, string code)
        {
            var app = BuildApp(principal);
            var result = await app.AcquireTokenByAuthorizationCode(_scopes, code).ExecuteAsync().ConfigureAwait(false);
            var account = await app.GetAccountAsync(principal.GetMsalAccountId());
            return result;
        }

        public async Task<string> GetAccessTokenAsync(ClaimsPrincipal principal)
        {
            var app = BuildApp(principal);
            var account = await app.GetAccountAsync(principal.GetMsalAccountId());

            // guest??
            if (null == account)
            {
                var accounts = await app.GetAccountsAsync();
                account = accounts.FirstOrDefault(a => a.Username == principal.GetLoginHint());
            }

            var token = await app.AcquireTokenSilent(_scopes, account).ExecuteAsync().ConfigureAwait(false);
            return token.AccessToken;
        }

        public void RemoveAccount(ClaimsPrincipal principal)
        {
            _userTokenCacheProviderFactory.Create(principal).Clear();
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

            _userTokenCacheProviderFactory.Create(principal).Initialize(app.UserTokenCache);

            return app;
        }
    }
}
