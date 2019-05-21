using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication.Extensions;
using WebApplication.Helper;

namespace WebApplication.Services
{
    public class ApiService
    {
        private readonly TokenService _tokenService;
        private readonly ApiOptions _apiOptions;
        private readonly AzureAdOptions _azureAdOptions;

        public ApiService(TokenService tokenService, IOptions<ApiOptions> apiOptions, IOptions<AzureAdOptions> adOptions)
        {
            _tokenService = tokenService;
            _apiOptions = apiOptions.Value;
            _azureAdOptions = adOptions.Value;
        }

        public async Task<string> GetClaims(ClaimsPrincipal principal)
        {
            try
            {
                var token = await _tokenService.GetAccessTokenAsync(principal, _azureAdOptions.ApiScopes);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return await client.GetStringAsync($"{_apiOptions.BaseUrl}/claims");
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> GetUserProfile(ClaimsPrincipal principal)
        {
            try
            {
                var token = await _tokenService.GetAccessTokenAsync(principal, _azureAdOptions.ApiScopes);
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return await client.GetStringAsync($"{_apiOptions.BaseUrl}/graph");
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
