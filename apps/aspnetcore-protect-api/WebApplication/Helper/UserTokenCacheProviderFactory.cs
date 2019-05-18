using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace WebApplication.Helper
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
