using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication.Extensions;

namespace WebApplication.Helper
{
    public class TokenService
    {
        private readonly AzureAdOptions _azureAdOptions;
        private readonly UserTokenCacheProviderFactory _userTokenCacheProviderFactory;

        public TokenService(IOptions<AzureAdOptions> options, UserTokenCacheProviderFactory userTokenCacheProviderFactory)
        {
            _azureAdOptions = options.Value;
            _userTokenCacheProviderFactory = userTokenCacheProviderFactory;
        }

        public async Task<AuthenticationResult> GetAccessTokenByAuthorizationCodeAsync(ClaimsPrincipal principal, string code, IEnumerable<string> scopes)
        {
            var app = BuildApp(principal);
            var result = await app.AcquireTokenByAuthorizationCode(scopes, code).ExecuteAsync().ConfigureAwait(false);
            return result;
        }

        public async Task<string> GetAccessTokenAsync(ClaimsPrincipal principal, IEnumerable<string> scopes)
        {
            var app = BuildApp(principal);
            var account = await app.GetAccountAsync(principal.GetMsalAccountId());

            // guest??
            if (null == account)
            {
                var accounts = await app.GetAccountsAsync();
                account = accounts.FirstOrDefault(a => a.Username == principal.GetLoginHint());
            }

            var token = await app.AcquireTokenSilent(scopes, account).ExecuteAsync().ConfigureAwait(false);
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
