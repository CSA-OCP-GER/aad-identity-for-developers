using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace aspnetcore_managed_identity_key_vault.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public TestController(IConfiguration configuration)  
        {  
            _configuration = configuration;  
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var secret1 = _configuration["mysecret1"];  
            var secret2 = _configuration["mysecret2"];
            return new string[] { secret1, secret2 };
        }
    }
}
