using System.Collections.Generic;

namespace WebApplication.Extensions
{
    public class AzureAdOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Instance { get; set; }
        public string Domain { get; set; }
        public string TenantId { get; set; }
        public string CallbackPath { get; set; }
        public string BaseUrl { get; set; }
        public List<string> ConsentScopes { get; set; }
        public List<string> ApiScopes { get; set; }
        public List<string> GraphScopes { get; set; }
    }
}
