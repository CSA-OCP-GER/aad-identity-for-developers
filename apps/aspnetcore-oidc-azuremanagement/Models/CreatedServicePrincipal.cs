using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.Models
{
    public class CreatedServicePrincipal
    {
        public string Id { get; set; }
        public string Displayname { get; set; }
        public bool Checked { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionId { get; set; }
    }
}
