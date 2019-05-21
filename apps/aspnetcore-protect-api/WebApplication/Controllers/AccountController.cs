﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace WebApplication.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly OpenIdConnectOptions _oidcOptions;

        public AccountController(IOptions<OpenIdConnectOptions> options)
        {
            _oidcOptions = options.Value;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            if (User.Identity.IsAuthenticated)
            {
                var callbackUrl = Url.Action(nameof(SignedOut), "Account", values: null, protocol: Request.Scheme);
                return SignOut(
                    new AuthenticationProperties { RedirectUri = callbackUrl },
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public IActionResult GrantConsent()
        {
            var scopes = new List<string>();
            foreach (var scope in _oidcOptions.Scope)
                scopes.Add(scope);

            scopes.Add("User.Read");


            var redirectUrl = Url.Action(nameof(HomeController.Index), "Home");
            return Challenge(
                new OpenIdConnectChallengeProperties
                {
                    RedirectUri = redirectUrl,
                    Prompt = "consent",
                    Scope = scopes
                },
                OpenIdConnectDefaults.AuthenticationScheme);
        }
    }
}