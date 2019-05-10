using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace aspnetcore_mvc_oauth2_code_grant.Helper
{
    public class GraphClientService
    {
        private readonly TokenService _tokenService;

        public GraphClientService(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<string> GetUserProfile(ClaimsPrincipal principal)
        {
            try
            {
                var token = await _tokenService.GetAccessToken(principal);

                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                return await client.GetStringAsync("https://graph.microsoft.com/v1.0/me");
            }
            catch (Exception e)
            {
                return e.Message;
            }
            
        }
    }
}
