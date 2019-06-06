using aspnetcore_oidc_azuremanagement.Dal;
using aspnetcore_oidc_azuremanagement.DomainObjects;
using aspnetcore_oidc_azuremanagement.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.Services
{
    public class AzureManagementService
    {
        private readonly TokenService _tokenService;
        private readonly AzureAdOptions _adOptions;
        private readonly HttpClient _httpClient;
        private readonly ServicePrincipalRepository _repository;

        public AzureManagementService(TokenService tokenService, IOptions<AzureAdOptions> adOptions, HttpClient httpClient, ServicePrincipalRepository repository)
        {
            _tokenService = tokenService;
            _adOptions = adOptions.Value;
            _httpClient = httpClient;
            _repository = repository;
        }

        public async Task<List<AzureSubscription>> GetSubscriptions(ClaimsPrincipal principal)
        {
            var token = await _tokenService.GetAccessTokenAsync(principal, _adOptions.ManagementAzureScope);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var result = await _httpClient.GetStringAsync("https://management.azure.com/subscriptions?api-version=2016-06-01");
            dynamic jsonResult = JsonConvert.DeserializeObject(result);

            var resultSubscriptions = new List<AzureSubscription>();

            foreach (var s in jsonResult.value)
            {
                resultSubscriptions.Add(new AzureSubscription
                {
                    Id = s.id,
                    SubscriptionId = s.subscriptionId,
                    Displayname = s.displayName
                });
            }
                        
            return resultSubscriptions;
        }

        public async Task AssignContributorRole(ClaimsPrincipal principal, DomainObjects.Subscription subscription, ServicePrincipal servicePrincipal)
        {
            // Contributor role: https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#contributor
            var contributorRoleId = "b24988ac-6180-42a0-ab88-20f7382dd24c";
            var assignmentId = Guid.NewGuid();
            var url = $"https://management.azure.com/subscriptions/{subscription.SubscriptionId}/providers/Microsoft.Authorization/roleAssignments/{assignmentId}?api-version=2015-07-01";
            var body = new
            {
                properties = new
                {
                    roleDefinitionId = $"/subscriptions/{subscription.SubscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{contributorRoleId}",
                    principalId = servicePrincipal.Id
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var token = await _tokenService.GetAccessTokenAsync(principal, _adOptions.ManagementAzureScope);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();

            await _repository.Add(subscription);
        }

        public async Task<List<string>> GetResourceGroups(ServicePrincipal servicePrincipal)
        {
            var badRequestException = false;
            var rgs = new List<string>(); 

            do
            {
                badRequestException = false;

                try
                {
                    var subscription = await _repository.GetSubscriptionByServicePrincipalId(servicePrincipal.Id);

                    var clientId = servicePrincipal.AppId;

                    var clientSecret = servicePrincipal.SecretText;

                    var token = await _tokenService.GetAccessTokenAsync(clientId, clientSecret, _adOptions.ManagementAzureDefault);

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    var result = await _httpClient.GetStringAsync($"https://management.azure.com/subscriptions/{subscription.SubscriptionId}/resourcegroups?api-version=2018-05-01");

                    dynamic jsonResult = JsonConvert.DeserializeObject(result);

                    foreach (var rg in jsonResult.value)
                    {
                        rgs.Add((string)rg.name);
                    }
                }
                catch (MsalServiceException e)
                {
                    if (e.StatusCode == 400)
                    {
                        badRequestException = true;
                        await Task.Delay(200);
                    }
                    else
                    {
                        throw;
                    }
                }                
            } while (badRequestException);

            return rgs;
        }

        public static IAsyncPolicy<HttpResponseMessage> CreateHttpClientPolicy()
        {
            // we interpret BadRequest (400) as transient fault, because it takes time until
            // the a newly created ServicePrincipal in all Azure APIs
            return HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .OrResult(msg => msg.StatusCode == HttpStatusCode.BadRequest)
                        .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}
