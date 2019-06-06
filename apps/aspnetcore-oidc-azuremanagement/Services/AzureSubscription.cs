using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.Services
{
    public class AzureSubscription
    {
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
        public string Displayname { get; set; }
    }
}
