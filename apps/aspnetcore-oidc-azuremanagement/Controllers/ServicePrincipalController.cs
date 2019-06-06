using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using aspnetcore_oidc_azuremanagement.Dal;
using aspnetcore_oidc_azuremanagement.Extensions;
using aspnetcore_oidc_azuremanagement.Models;
using aspnetcore_oidc_azuremanagement.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace aspnetcore_oidc_azuremanagement.Controllers
{
    [Authorize]
    public class ServicePrincipalController : Controller
    {
        private readonly TokenService _tokenService;
        private readonly AzureAdOptions _adOptions;
        private readonly GraphService _graphService;
        private readonly OpenIdConnectOptions _oidcOptions;
        private readonly AzureManagementService _azureManagementService;
        private readonly ServicePrincipalRepository _repository;

        public ServicePrincipalController(
            TokenService tokenService, 
            GraphService graphService, 
            IOptions<AzureAdOptions> adOptions,
            IOptions<OpenIdConnectOptions> oidcOptions,
            AzureManagementService azureManagementService,
            ServicePrincipalRepository repository)
        {
            _tokenService = tokenService;
            _graphService = graphService;
            _adOptions = adOptions.Value;
            _oidcOptions = oidcOptions.Value;
            _azureManagementService = azureManagementService;
            _repository = repository;
        }

        public async Task<IActionResult> Index()
        {
            var sps = await _repository.GetServicePrincipals();
            var subscriptions = (await _repository.GetSubscriptionsByServicePrincipals(sps.Select(sp => sp.Id).ToList())).ToDictionary(s => s.ServicePrincipalId);

            return View(sps.Select(sp => new CreatedServicePrincipal
            {
                Id = sp.Id,
                Displayname = sp.DisplayName,
                Checked = false,
                SubscriptionId = subscriptions[sp.Id].SubscriptionId,
                SubscriptionName = subscriptions[sp.Id].Displayname
            }).ToList());
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var token = await _tokenService.GetAccessTokenAsync(User, _adOptions.MsGraphScope);
            }
            catch (MsalUiRequiredException e)
            {
                // User has not granted consent to 'https://graph.microsoft.com/Directory.AccessAsUser.All'.
                // We must gain user's consent to access 'https://graph.microsoft.com/Directory.AccessAsUser.All'.
                var scopes = new List<string>();
                scopes.Add("openid");
                scopes.Add("profile");
                scopes.Add("offline_access");
                scopes.Add(_adOptions.MsGraphScope);
                scopes.Add(_adOptions.ManagementAzureScope);

                var redirectUrl = Url.Action(nameof(ServicePrincipalController.Create), "ServicePrincipal");

                return Challenge(
                    new OpenIdConnectChallengeProperties(new Dictionary<string, string>
                    {
                        { "ConsentChallengeController", "ServicePrincipal" },
                        { "ActionOnError", Url.Action(nameof(ServicePrincipalController.ConsentDenied), "ServicePrincipal") }
                    })
                    {
                        RedirectUri = redirectUrl,
                        Prompt = "consent",
                        Scope = scopes
                    },
                    OpenIdConnectDefaults.AuthenticationScheme);
            }
            
            var subscriptions = await _azureManagementService.GetSubscriptions(User);
            ViewBag.Subscriptions = subscriptions;
            return View();
        }

        public IActionResult ConsentDenied()
        {
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Create(Subscription subscription)
        {
            var sp = await _graphService.CreateServicePrincipal(User);
            // Get all Azure Subscription to which the user has access to and map it to a domain object
            var sub = (await _azureManagementService.GetSubscriptions(User))
                .Where(s => s.Id == subscription.Id)
                .Select(s => new DomainObjects.Subscription
                {
                    Id = s.Id,
                    SubscriptionId = s.SubscriptionId,
                    Displayname = s.Displayname,
                    ServicePrincipalId = sp.Id
                }).Single();

            await _azureManagementService.AssignContributorRole(User, sub, sp);
            var rgs = await _azureManagementService.GetResourceGroups(sp);

            return View("ResourceGroups", rgs);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(List<CreatedServicePrincipal> principals)
        {
            var set = principals.Select(sp => sp.Id).ToHashSet();
            var sps = await _repository.GetServicePrincipals(principals.Where(p => p.Checked).Select(p => p.Id));

            await _graphService.Delete(User, sps);

            await _repository.Delete(sps);            

            var spsForModel = await _repository.GetServicePrincipals();
            var subscriptions = (await _repository.GetSubscriptionsByServicePrincipals(spsForModel.Select(sp => sp.Id))).ToDictionary(s => s.ServicePrincipalId);

            var model = spsForModel.Select(sp => new CreatedServicePrincipal
            {
                Id = sp.Id,
                Displayname = sp.DisplayName,
                Checked = false,
                SubscriptionId = subscriptions[sp.Id].SubscriptionId,
                SubscriptionName = subscriptions[sp.Id].Displayname
            }).ToList();

            return View("Index", model);
        }

        public async Task<IActionResult> GetResourceGroups(string servicePrincipalId)
        {
            var sp = await _repository.Get(servicePrincipalId);
            var rgs = await _azureManagementService.GetResourceGroups(sp);
            return View("ResourceGroups", rgs);
        }
    }
}