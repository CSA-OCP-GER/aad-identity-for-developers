using System.Security.Claims;

namespace aspnetcore_mvc_oauth2_code_grant.Helper
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
    }
}
