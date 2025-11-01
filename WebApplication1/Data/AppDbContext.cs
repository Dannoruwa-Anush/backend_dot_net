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
        public DbSet<Category> Categories { get; set; }
        public DbSet<ElectronicItem> ElectronicItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderElectronicItem> CustomerOrderElectronicItems { get; set; }
        public DbSet<Cashflow> Cashflows { get; set; }
        public DbSet<BNPL_PlanType> BNPL_PlanTypes { get; set; }
        public DbSet<BNPL_PLAN> BNPL_PLANs { get; set; }
        //---


        
        
        

        
        
        public DbSet<BNPL_Installment> BNPL_Installments { get; set; }
        

        //-------- [Start: configure model] -----------
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------------------------------------------------
            // Brand
            // -------------------------------------------------------------
            modelBuilder.Entity<Brand>(entity =>
            {
                entity.HasIndex(b => b.BrandName).IsUnique();

                // (1) — (M) ElectronicItem
                entity.HasMany(b => b.ElectronicItems)
                      .WithOne(i => i.Brand)
                      .HasForeignKey(i => i.BrandID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -------------------------------------------------------------
            // Category
            // -------------------------------------------------------------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.CategoryName).IsUnique();

                // (1) — (M) ElectronicItem
                entity.HasMany(c => c.ElectronicItems)
                      .WithOne(i => i.Category)
                      .HasForeignKey(i => i.CategoryID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -------------------------------------------------------------
            // ElectronicItem
            // -------------------------------------------------------------
            modelBuilder.Entity<ElectronicItem>(entity =>
            {
                entity.HasIndex(i => i.E_ItemName).IsUnique();

                entity.Property(i => i.Price)
                      .HasColumnType("decimal(18,2)");

                // (1) — (M) CustomerOrderElectronicItem
                entity.HasMany(i => i.CustomerOrderElectronicItems)
                      .WithOne(oi => oi.ElectronicItem)
                      .HasForeignKey(oi => oi.E_ItemID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------------------------------------------
            // Customer
            // -------------------------------------------------------------
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => c.Email).IsUnique();

                // (1) — (M) CustomerOrder
                entity.HasMany(c => c.CustomerOrders)
                      .WithOne(o => o.Customer)
                      .HasForeignKey(o => o.CustomerID)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // -------------------------------------------------------------
            // CustomerOrder
            // -------------------------------------------------------------
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)");

                // (1) — (M) Cashflow
                entity.HasMany(o => o.Cashflows)
                      .WithOne(p => p.CustomerOrder)
                      .HasForeignKey(p => p.OrderID)
                      .OnDelete(DeleteBehavior.Cascade);

                // (1) — (0..1) BNPL_PLAN
                entity.HasOne(o => o.BNPL_PLAN)
                      .WithOne(p => p.CustomerOrder)
                      .HasForeignKey<BNPL_PLAN>(p => p.OrderID)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);

                // (1) — (M) CustomerOrderElectronicItem
                entity.HasMany(o => o.CustomerOrderElectronicItems)
                      .WithOne(oi => oi.CustomerOrder)
                      .HasForeignKey(oi => oi.OrderID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // -------------------------------------------------------------
            // CustomerOrderElectronicItem
            // -------------------------------------------------------------
            modelBuilder.Entity<CustomerOrderElectronicItem>(entity =>
            {
                // Composite primary key : OrderID and E_ItemID
                entity.HasKey(oi => new { oi.OrderID, oi.E_ItemID });

                entity.Property(oi => oi.UnitPrice)
                      .HasColumnType("decimal(18,2)");

                entity.Property(oi => oi.SubTotal)
                       .HasColumnType("decimal(18,2)");         
            });

            // -------------------------------------------------------------
            // Cashflow
            // -------------------------------------------------------------
            modelBuilder.Entity<Cashflow>(entity =>
            {
                entity.Property(p => p.AmountPaid)
                      .HasColumnType("decimal(18,2)");
            });

            // -------------------------------------------------------------
            // BNPL_PlanType
            // -------------------------------------------------------------
            modelBuilder.Entity<BNPL_PlanType>(entity =>
            {
                entity.Property(p => p.InterestRate)
                      .HasColumnType("decimal(5,2)");

                entity.Property(p => p.LatePayInterestRate)
                      .HasColumnType("decimal(5,2)");

                // (1) — (M) BNPL_PLAN handled in BNPL_PLAN entity
            });
            
            // -------------------------------------------------------------
            // BNPL_PLAN
            // -------------------------------------------------------------
            modelBuilder.Entity<BNPL_PLAN>(entity =>
            {
                entity.Property(p => p.Bnpl_AmountPerInstallment)
                      .HasColumnType("decimal(18,2)");

                // (1) — (M) BNPL_Installment
                entity.HasMany(p => p.BNPL_Installments)
                      .WithOne(i => i.BNPL_PLAN)
                      .HasForeignKey(i => i.Bnpl_PlanID)
                      .OnDelete(DeleteBehavior.Cascade);

                // (M) — (1) BNPL_PlanType
                entity.HasOne(p => p.BNPL_PlanType)
                      .WithMany(pt => pt.BNPL_PLANs)
                      .HasForeignKey(p => p.Bnpl_PlanTypeID)
                      .OnDelete(DeleteBehavior.Restrict);
            });






            // -------------------------------------------------------------
            // BNPL_Installment
            // -------------------------------------------------------------
            modelBuilder.Entity<BNPL_Installment>(entity =>
            {
                entity.Property(i => i.Installment_BaseAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.OverPaymentCarried)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.ArrearsCarried)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.TotalDueAmount)
                    .HasColumnType("decimal(18,2)");
                      
                entity.Property(i => i.AmountPaid)
                      .HasColumnType("decimal(18,2)");
            });


        }
        //-------- [End: configure model] -------------



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