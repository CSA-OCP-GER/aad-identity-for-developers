using aspnetcore_oidc_azuremanagement.Dal;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.Services
{
    public class GraphService
    {
        private readonly TokenService _tokenService;
        private readonly ServicePrincipalRepository _repository;

        public GraphService(TokenService tokenService, ServicePrincipalRepository repository)
        {
            _tokenService = tokenService;
            _repository = repository;
        }

        public async Task<DomainObjects.ServicePrincipal> CreateServicePrincipal(ClaimsPrincipal principal)
        {
            // create a password for the ServicePrincipal
            var secret = Guid.NewGuid().ToString();

            // create the GraphClient
            var client = new GraphServiceClient(new DelegateAuthenticationProvider(async requestMessage => 
            {
                var token = await _tokenService.GetAccessTokenAsync(principal, "https://graph.microsoft.com/Directory.AccessAsUser.All");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }));

            // Create the Azure AD application
            var app = await client.Applications.Request().AddAsync(new Application
            {
                DisplayName = $"demosprbac-{Guid.NewGuid()}",
                PasswordCredentials = new List<PasswordCredential>()
                {
                    new PasswordCredential()
                    {
                        DisplayName = "ClientSecret",
                        SecretText = secret,
                        StartDateTime = DateTimeOffset.Now.AddMinutes(-2),
                        EndDateTime = DateTimeOffset.Now.AddYears(1)
                    }
                }
            });

            // now create an instance of the application in the current Azure AD tenant, this is the ServicePrincipal
            var sp = await client.ServicePrincipals.Request().AddAsync(new ServicePrincipal()
            {
                AppId = app.AppId
            });

            return await _repository.Add(new DomainObjects.ServicePrincipal
            {
                Id = sp.Id,
                AppId = sp.AppId,
                AppObjectId = app.Id,
                DisplayName = sp.DisplayName,
                SecretText = secret 
            });
        }

        public async Task<List<DomainObjects.ServicePrincipal>> Delete(ClaimsPrincipal principal, List<DomainObjects.ServicePrincipal> principals)
        {
            // create the GraphClient
            var client = new GraphServiceClient(new DelegateAuthenticationProvider(async requestMessage =>
            {
                var token = await _tokenService.GetAccessTokenAsync(principal, "https://graph.microsoft.com/Directory.AccessAsUser.All");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }));

            foreach (var sp in principals)
            {
                var theApp = await client.Applications[sp.AppObjectId].Request().GetAsync();
                await client.Applications[sp.AppObjectId].Request().DeleteAsync();
            }

            return principals;
        }
    }
}
