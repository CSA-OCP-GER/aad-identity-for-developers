using aspnetcore_mvc_oauth2_code_grant.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace aspnetcore_mvc_oauth2_code_grant.Helper
{
    public class TokenService
    {
        private static object _syncRoot = new object();
        private static Dictionary<string, byte[]> _tokenCache = new Dictionary<string, byte[]>();
        private readonly AzureAdOptions _azureAdOptions;
        private readonly string[] _scopes = new[] { "https://graph.microsoft.com/User.Read" };

        public TokenService(IOptions<AzureAdOptions> options)
        {
            _azureAdOptions = options.Value;
        }

        public async Task<AuthenticationResult> GetAccessTokenByAuthorizationCode(ClaimsPrincipal principal, string code)
        {
            var app = BuildApp();
            var result = await app.AcquireTokenByAuthorizationCode(_scopes, code).ExecuteAsync().ConfigureAwait(false);
            SaveToCache(principal, app);
            var account = await app.GetAccountAsync(principal.GetMsalAccountId());
            return result;
        }

        public async Task<string> GetAccessToken(ClaimsPrincipal principal)
        {
            var app = BuildApp();
            LoadFromCache(principal, app);
            var account = await app.GetAccountAsync(principal.GetMsalAccountId());
            var token = await app.AcquireTokenSilent(_scopes, account).ExecuteAsync().ConfigureAwait(false);
            SaveToCache(principal, app);
            return token.AccessToken;
        }

        public void RemoveAccount(ClaimsPrincipal principal)
        {
            lock (_syncRoot)
                _tokenCache.Remove(principal.GetMsalAccountId());
        }

        private IConfidentialClientApplication BuildApp()
        {
            return ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                .WithClientSecret(_azureAdOptions.ClientSecret)
                .WithAuthority(AzureCloudInstance.AzurePublic, Guid.Parse(_azureAdOptions.TenantId))
                .WithRedirectUri(_azureAdOptions.BaseUrl + _azureAdOptions.CallbackPath)
                .Build();
        }

        private void SaveToCache(ClaimsPrincipal principal, IConfidentialClientApplication app)
        {
            var data = app.UserTokenCache.SerializeMsalV3();

            lock (_syncRoot)
                _tokenCache[principal.GetMsalAccountId()] = data;
        }

        private void LoadFromCache(ClaimsPrincipal principal, IConfidentialClientApplication app)
        {
            lock (_syncRoot)
            {
                if (!_tokenCache.ContainsKey(principal.GetMsalAccountId()))
                    throw new InvalidOperationException();

                app.UserTokenCache.DeserializeMsalV3(_tokenCache[principal.GetMsalAccountId()]);
            }
        }
    }
}
