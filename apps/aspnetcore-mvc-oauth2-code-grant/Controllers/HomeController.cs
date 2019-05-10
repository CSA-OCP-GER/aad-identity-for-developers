using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using aspnetcore_mvc_oauth2_code_grant.Models;
using aspnetcore_mvc_oauth2_code_grant.Helper;
using System.Threading.Tasks;

namespace aspnetcore_mvc_oauth2_code_grant.Controllers
{
    public class HomeController : Controller
    {
        private readonly TokenService _tokenService;
        private readonly GraphClientService _graphService;
        public HomeController(TokenService tokenService, GraphClientService graphService)
        {
            _tokenService = tokenService;
            _graphService = graphService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var profile = await _graphService.GetUserProfile(User);

                return View(new UserModel { Name = profile });
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
