using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApi.Extensions;
using WebApi.Helper;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private readonly string _graphBaseUrl = "https://graph.microsoft.com";
        private readonly TokenService _tokenService;
        private readonly AzureAdOptions _azureAdOptions;

        public GraphController(TokenService tokenService, IOptions<AzureAdOptions> azureAdOptions)
        {
            _tokenService = tokenService;
            _azureAdOptions = azureAdOptions.Value;
        }

        [HttpGet]
        [SwaggerOperation("GetUserProfile")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<string> GetUserProfile()
        {
            var client = new HttpClient();
            var token = await _tokenService.GetAccessTokenAsync(this.HttpContext.User, _azureAdOptions.GraphScopes);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await client.GetStringAsync($"{_graphBaseUrl}/v1.0/me");
        }
    }
}
