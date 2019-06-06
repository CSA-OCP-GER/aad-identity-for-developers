using System.Security.Claims;

namespace aspnetcore_oidc_azuremanagement.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        private const string ObjectId = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string TenantId = "http://schemas.microsoft.com/identity/claims/tenantid";

        public static string GetMsalAccountId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = claimsPrincipal.GetObjectId();
            string tenantId = claimsPrincipal.GetTenantId();

            if (!string.IsNullOrWhiteSpace(userObjectId) && !string.IsNullOrWhiteSpace(tenantId))
            {
                return $"{userObjectId}.{tenantId}";
            }

            return null;
        }

        public static string GetObjectId(this ClaimsPrincipal claimsPrincipal)
        {
            string userObjectId = claimsPrincipal.FindFirstValue(ObjectId);
            if (string.IsNullOrEmpty(userObjectId))
            {
                userObjectId = claimsPrincipal.FindFirstValue("oid");
            }

            return userObjectId;
        }

        public static string GetTenantId(this ClaimsPrincipal claimsPrincipal)
        {
            string tenantId = claimsPrincipal.FindFirstValue(TenantId);
            if (string.IsNullOrEmpty(tenantId))
            {
                tenantId = claimsPrincipal.FindFirstValue("tid");
            }

            return tenantId;
        }

        public static string GetLoginHint(this ClaimsPrincipal claimsPrincipal)
        {
            var displayName = claimsPrincipal.FindFirstValue("preferred_username");

            // Otherwise falling back to the claims brought by an Azure AD v1.0 token
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue(ClaimsIdentity.DefaultNameClaimType);
            }

            // Finally falling back to name
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = claimsPrincipal.FindFirstValue("name");
            }

            return displayName;
        }

        public static string GetDomainHint(this ClaimsPrincipal claimsPrincipal)
        {
            // Tenant for MSA accounts
            const string msaTenantId = "9188040d-6c67-4c5b-b112-36a304b66dad";

            var tenantId = GetTenantId(claimsPrincipal);
            string domainHint = string.IsNullOrWhiteSpace(tenantId) ? null :
                tenantId == msaTenantId ? "consumers" : "organizations";
            return domainHint;
        }
    }
}
