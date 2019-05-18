using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication.Helper;

namespace WebApplication.Services
{
    public class ApiService
    {
        private readonly TokenService _tokenService;
        private readonly ApiOptions _apiOptions;

        public ApiService(TokenService tokenService, IOptions<ApiOptions> options)
        {
            _tokenService = tokenService;
            _apiOptions = options.Value;
        }

        public async Task<string> GetClaims(ClaimsPrincipal principal)
        {
            try
            {
                var token = await _tokenService.GetAccessTokenAsync(principal);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return await client.GetStringAsync(_apiOptions.BaseUrl);
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }
    }
}
