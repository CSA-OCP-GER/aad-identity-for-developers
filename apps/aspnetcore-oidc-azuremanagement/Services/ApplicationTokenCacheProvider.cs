using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.Services
{
    public class ApplicationTokenCacheProvider
    {
        private readonly IMemoryCache _cache;
        private readonly string _key = "apptokencache";

        public ApplicationTokenCacheProvider(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Initialize(ITokenCache tokenCache)
        {
            tokenCache.SetBeforeAccess(BeforeAccessNotification);
            tokenCache.SetAfterAccess(AfterAccessNotification);
            tokenCache.SetBeforeWrite(BeforeWriteNotification);
        }

        public void Clear()
        {
            _cache.Remove(_key);
        }

        private void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (args.HasStateChanged)
            {
                Persist(_key, args.TokenCache);
            }
        }

        private void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load(_key, args.TokenCache);
        }

        private void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
        }

        private void Persist(string key, ITokenCache tokenCache)
        {
            _cache.Set(key, tokenCache.SerializeMsalV3(), DateTimeOffset.Now.AddHours(12));
        }

        private void Load(string key, ITokenCache tokenCache)
        {
            var data = (byte[])_cache.Get(key);
            tokenCache.DeserializeMsalV3(data);
        }
    }
}
