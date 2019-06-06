using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace aspnetcore_oidc_azuremanagement.Services
{
    public class TokenCacheproviderFactory
    {
        private readonly IMemoryCache _memoryCache;

        public TokenCacheproviderFactory(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public UserTokenCacheProvider Create(ClaimsPrincipal principal)
        {
            return new UserTokenCacheProvider(_memoryCache, principal);
        }

        public ApplicationTokenCacheProvider Create()
        {
            return new ApplicationTokenCacheProvider(_memoryCache);
        }

        public ServicePrincipalTokenCache Create(string clientId)
        {
            return new ServicePrincipalTokenCache(_memoryCache, clientId);
        }
    }
}
