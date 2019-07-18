using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace token_echo_server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminConsentController : ControllerBase
    {
        public async Task<IActionResult> AdminConsented()
        {
            return Ok(await Task.FromResult(this.Request.QueryString.Value));
        }
    }
}