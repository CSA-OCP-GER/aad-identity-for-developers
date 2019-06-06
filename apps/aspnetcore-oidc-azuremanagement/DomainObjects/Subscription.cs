using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.DomainObjects
{
    public class Subscription
    {
        public string ServicePrincipalId { get; set; }
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
        public string Displayname { get; set; }
    }
}
