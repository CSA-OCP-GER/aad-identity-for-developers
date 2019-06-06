using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using aspnetcore_oidc_azuremanagement.Models;
using aspnetcore_oidc_azuremanagement.Services;

namespace aspnetcore_oidc_azuremanagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly GraphService _graphService;

        public HomeController(GraphService graphService)
        {
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
