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
        public HomeController(ApiService apiService)
        {
            _apiService = apiService;
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
            var profile = await _apiService.GetUserProfile(HttpContext.User);
            return View("Index", new IndexModel
            {
                Claims = string.Empty,
                Profile = profile
            });            
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
