using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using System.Text;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ClaimsController : ControllerBase
    {
        [HttpGet]
        [SwaggerOperation("GetClaims")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public IActionResult GetClaims()
        {
            var result = string.Empty;
            var builder = new StringBuilder();

            foreach (var claim in HttpContext.User.Claims)
            {
                builder.AppendLine($"{claim.Type} : {claim.Value}");
            }

            result = builder.ToString();
            return Ok(result);
        }
    }
}
