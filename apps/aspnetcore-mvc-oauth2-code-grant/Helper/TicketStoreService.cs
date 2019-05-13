using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace aspnetcore_mvc_oauth2_code_grant.Helper
{
    public class TicketStoreService : ITicketStore
    {
        private readonly IMemoryCache _cache;
        private const string _keyPrefix = "authticket";

        public TicketStoreService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.FromResult(true);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var options = new MemoryCacheEntryOptions();
            var expiresUtc = ticket.Properties.ExpiresUtc;
            if (expiresUtc.HasValue)
            {
                options.SetAbsoluteExpiration(expiresUtc.Value);
            }

            options.SetSlidingExpiration(TimeSpan.FromHours(1));

            _cache.Set(key, ticket, options);

            return Task.FromResult(true);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            AuthenticationTicket ticket;
            _cache.TryGetValue(key, out ticket);
            return Task.FromResult(ticket);
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var id = Guid.NewGuid();
            var key = _keyPrefix + id;
            await((ITicketStore)this).RenewAsync(key, ticket);
            return key;
        }
    }
}
