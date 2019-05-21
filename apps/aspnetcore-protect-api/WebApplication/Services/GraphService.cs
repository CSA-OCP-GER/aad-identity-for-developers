using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication.Extensions;
using WebApplication.Helper;

namespace WebApplication.Services
{
    public class GraphService
    {
        private readonly AzureAdOptions _adOptions;
        private readonly TokenService _tokenService;
        private readonly string _graphUrl = "https://graph.microsoft.com";

        public GraphService(TokenService tokenService, IOptions<AzureAdOptions> adOptions)
        {
            _tokenService = tokenService;
            _adOptions = adOptions.Value;
        }

        public async Task<string> GetUserProfile(ClaimsPrincipal principal)
        {
            try
            {
                var token = await _tokenService.GetAccessTokenAsync(principal, _adOptions.GraphScopes);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return await client.GetStringAsync($"{_graphUrl}/v1.0/me");
            }
            catch (MsalUiRequiredException)
            {
                throw;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
    }
}
