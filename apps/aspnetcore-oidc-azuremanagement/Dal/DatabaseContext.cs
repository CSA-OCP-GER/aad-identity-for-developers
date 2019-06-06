using aspnetcore_oidc_azuremanagement.DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace aspnetcore_oidc_azuremanagement.Dal
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {

        }

        public DbSet<ServicePrincipal> ServicePrincipals { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var spEntity = builder.Entity<ServicePrincipal>()
                .ToTable("serviceprincipal");

            spEntity.Property(sp => sp.Id).HasColumnName("id");
            spEntity.Property(sp => sp.AppId).HasColumnName("appid");
            spEntity.Property(sp => sp.AppObjectId).HasColumnName("appobjectid");
            spEntity.Property(sp => sp.DisplayName).HasColumnName("displayname");
            spEntity.Property(sp => sp.SecretText).HasColumnName("secrettext");

            var subEntity = builder.Entity<Subscription>()
                .ToTable("Subscription");

            subEntity.HasKey(sub => sub.ServicePrincipalId).HasName("serviceprincipalid");
            subEntity.Property(sub => sub.SubscriptionId).HasColumnName("subscriptionid");
            subEntity.Property(sub => sub.Id).HasColumnName("id");
        }
    }
}
