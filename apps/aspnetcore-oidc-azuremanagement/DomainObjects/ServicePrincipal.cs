namespace aspnetcore_oidc_azuremanagement.DomainObjects
{
    public class ServicePrincipal
    {
        public string Id { get; set; }
        public string AppId { get; set; }
        public string AppObjectId { get; set; }
        public string DisplayName { get; set; }
        public string SecretText { get; set; }
    }
}
