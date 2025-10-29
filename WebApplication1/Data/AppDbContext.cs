using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Utils.Helpers;

namespace WebApplication1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //Tables in DB.
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BNPL_PlanType> BNPL_PlanTypes { get; set; }




        //-------- [Start: Intercept All DateTime Before Save] -----------
        public override int SaveChanges()
        {
            ApplySriLankaTimeZone();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySriLankaTimeZone();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplySriLankaTimeZone()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue is DateTime dateTimeValue)
                    {
                        // Convert UTC or unspecified times to Sri Lanka time
                        if (dateTimeValue.Kind == DateTimeKind.Utc || dateTimeValue.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = TimeZoneHelper.ToSriLankaTime(dateTimeValue);
                        }
                    }
                }
            }
        }
        //-------- [End: Intercept All DateTime Before Save] -------------
    }
}