using aspnetcore_oidc_azuremanagement.DomainObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aspnetcore_oidc_azuremanagement.Dal
{
    public class ServicePrincipalRepository
    {
        private readonly DatabaseContext _db;

        public ServicePrincipalRepository(DatabaseContext db)
        {
            _db = db;
        }

        public async Task<ServicePrincipal> Add(ServicePrincipal sp)
        {
            _db.ServicePrincipals.Add(sp);
            await _db.SaveChangesAsync();
            return sp;
        }

        public async Task<ServicePrincipal> Get(string id)
        {
            return await _db.ServicePrincipals.FirstAsync(sp => sp.Id == id);
        }

        public async Task<List<ServicePrincipal>> GetServicePrincipals()
        {
            return await _db.ServicePrincipals.ToListAsync();
        }

        public async Task<List<ServicePrincipal>> GetServicePrincipals(IEnumerable<string> ids)
        {
            var set = new HashSet<string>(ids);
            return await _db.ServicePrincipals.Where(sp => set.Contains(sp.Id)).ToListAsync();
        }

        public async Task Delete(IEnumerable<ServicePrincipal> principals)
        {
            _db.ServicePrincipals.RemoveRange(principals);

            var subscriptions = await GetSubscriptionsByServicePrincipals(principals.Select(sp => sp.Id));
            _db.Subscriptions.RemoveRange(subscriptions);

            await _db.SaveChangesAsync();
        }

        public async Task<Subscription> Add(Subscription subscription)
        {
            _db.Subscriptions.Add(subscription);
            await _db.SaveChangesAsync();
            return subscription;
        }

        public async Task<Subscription> GetSubscriptionByServicePrincipalId(string id)
        {
            return await _db.Subscriptions.Where(s => s.ServicePrincipalId == id).SingleAsync();
        }

        public async Task<List<Subscription>> GetSubscriptionsByServicePrincipals(IEnumerable<string> spIds)
        {
            var set = new HashSet<string>(spIds);
            return await _db.Subscriptions.Where(s => set.Contains(s.ServicePrincipalId)).ToListAsync();
        }
    }
}
