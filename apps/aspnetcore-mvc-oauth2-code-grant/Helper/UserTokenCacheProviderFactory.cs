using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace aspnetcore_mvc_oauth2_code_grant.Helper
{
    public class UserTokenCacheProviderFactory
    {
        private readonly IMemoryCache _memoryCache;

        public UserTokenCacheProviderFactory(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public UserTokenCacheProvider Create(ClaimsPrincipal principal)
        {
            return new UserTokenCacheProvider(_memoryCache, principal);
        }
    }
}
