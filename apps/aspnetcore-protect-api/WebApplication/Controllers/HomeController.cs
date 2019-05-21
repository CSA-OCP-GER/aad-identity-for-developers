using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _apiService;
        private readonly GraphService _graphService;

        public HomeController(ApiService apiService, GraphService graphService)
        {
            _apiService = apiService;
            _graphService = graphService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetClaims()
        {
            var claims = await _apiService.GetClaims(this.HttpContext.User);
            return View("Index", new IndexModel
            {
                Profile = string.Empty,
                Claims = claims
            });
        }

        [Authorize]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var profile = await _graphService.GetUserProfile(this.HttpContext.User);
                return View("Index", new IndexModel
                {
                    Claims = string.Empty,
                    Profile = profile
                });
            }
            catch (MsalUiRequiredException e)
            {
                if (e.ErrorCode == "invalid_grant")
                    return Redirect("/Home/ConsentRequired");

                return View("Index", new IndexModel
                {
                    Claims = string.Empty,
                    Profile = e.ToString()
                });
            }            
        }

        [Authorize]
        public IActionResult ConsentRequired()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
