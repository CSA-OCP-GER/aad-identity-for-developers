using System.Collections.Generic;

namespace WebApi.Extensions
{
    public class AzureAdOptions
    {
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientIdUri { get; set; }
        public string ClientSecret { get; set; }
        public List<string> GraphScopes { get; set; }
    }
}
