using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.Base;
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
        public DbSet<User> Users { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerOrder> CustomerOrders { get; set; }
        public DbSet<CustomerOrderElectronicItem> CustomerOrderElectronicItems { get; set; }
        public DbSet<Cashflow> Cashflows { get; set; }
        public DbSet<BNPL_PlanType> BNPL_PlanTypes { get; set; }
        public DbSet<BNPL_PLAN> BNPL_PLANs { get; set; }
        public DbSet<BNPL_Installment> BNPL_Installments { get; set; }
        public DbSet<BNPL_PlanSettlementSummary> BNPL_PlanSettlementSummaries{ get; set; }
        //---

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
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related ElectronicItems exist
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
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related ElectronicItems exist
            });

            // -------------------------------------------------------------
            // ElectronicItem
            // -------------------------------------------------------------
            modelBuilder.Entity<ElectronicItem>(entity =>
            {
                entity.HasIndex(i => i.ElectronicItemName).IsUnique();

                entity.Property(i => i.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.RowVersion)
                    .HasColumnType("BINARY(8)")
                    .IsRequired()
                    .IsConcurrencyToken();

                // (1) — (M) CustomerOrderElectronicItem
                entity.HasMany(i => i.CustomerOrderElectronicItems)
                      .WithOne(oi => oi.ElectronicItem)
                      .HasForeignKey(oi => oi.E_ItemID)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related CustomerOrderElectronicItems exist
            });

            // -------------------------------------------------------------
            // User
            // -------------------------------------------------------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();

                // (1) — (1) Employee
                entity.HasOne(u => u.Employee)
                      .WithOne(e => e.User)
                      .HasForeignKey<Employee>(e => e.UserID)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related Employee exist

                // (1) — (1) Customer
                entity.HasOne(u => u.Customer)
                     .WithOne(c => c.User)
                     .HasForeignKey<Customer>(c => c.UserID)
                     .IsRequired(false)
                     .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related Customer exist
            });

            // -------------------------------------------------------------
            // Employee
            // -------------------------------------------------------------
            modelBuilder.Entity<Employee>(entity =>
            {
                // (1) — (1) User handled in User entity
            });

            // -------------------------------------------------------------
            // Customer
            // -------------------------------------------------------------
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(c => c.PhoneNo).IsUnique();

                // (1) — (M) CustomerOrder
                entity.HasMany(c => c.CustomerOrders)
                      .WithOne(o => o.Customer)
                      .HasForeignKey(o => o.CustomerID)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related CustomerOrders exist
            });

            // -------------------------------------------------------------
            // CustomerOrder
            // -------------------------------------------------------------
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(o => o.RowVersion)
                    .HasColumnType("BINARY(8)")
                    .IsRequired()
                    .IsConcurrencyToken();   

                // (1) — (M) Cashflow
                entity.HasMany(o => o.Cashflows)
                      .WithOne(p => p.CustomerOrder)
                      .HasForeignKey(p => p.OrderID)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related Cashflows exist

                // (1) — (0..1) BNPL_PLAN
                entity.HasOne(o => o.BNPL_PLAN)
                      .WithOne(p => p.CustomerOrder)
                      .HasForeignKey<BNPL_PLAN>(p => p.OrderID)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related BNPL_PLAN exist

                // (1) — (M) CustomerOrderElectronicItem
                entity.HasMany(o => o.CustomerOrderElectronicItems)
                      .WithOne(oi => oi.CustomerOrder)
                      .HasForeignKey(oi => oi.OrderID)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related CustomerOrderElectronicItems exist
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

                entity.Property(oi => oi.RowVersion)
                    .HasColumnType("BINARY(8)")
                    .IsRequired()
                    .IsConcurrencyToken();    
            });

            // -------------------------------------------------------------
            // Cashflow
            // -------------------------------------------------------------
            modelBuilder.Entity<Cashflow>(entity =>
            {
                entity.HasIndex(c => c.CashflowRef).IsUnique();

                entity.Property(c => c.AmountPaid)
                      .HasColumnType("decimal(18,2)");

                entity.Property(c => c.RowVersion)
                    .HasColumnType("BINARY(8)")
                    .IsRequired()
                    .IsConcurrencyToken();
            });

            // -------------------------------------------------------------
            // BNPL_PlanType
            // -------------------------------------------------------------
            modelBuilder.Entity<BNPL_PlanType>(entity =>
            {
                entity.HasIndex(p => p.Bnpl_PlanTypeName).IsUnique();

                entity.Property(p => p.InterestRate)
                      .HasColumnType("decimal(5,2)");

                entity.Property(p => p.LatePayInterestRatePerDay)
                      .HasColumnType("decimal(5,2)");

                // (1) — (M) BNPL_PLAN handled in BNPL_PLAN entity
            });

            // -------------------------------------------------------------
            // BNPL_PLAN
            // -------------------------------------------------------------
            modelBuilder.Entity<BNPL_PLAN>(entity =>
            {
                entity.Property(p => p.Bnpl_InitialPayment)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.Bnpl_AmountPerInstallment)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.RowVersion)
                    .HasColumnType("BINARY(8)")
                    .IsRequired()
                    .IsConcurrencyToken();

                // (1) — (M) BNPL_Installment
                entity.HasMany(p => p.BNPL_Installments)
                      .WithOne(i => i.BNPL_PLAN)
                      .HasForeignKey(i => i.Bnpl_PlanID)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related BNPL_Installments exist

                //(1) — BNPL_PlanSettlementSummary(M)
                entity.HasMany(p => p.BNPL_PlanSettlementSummaries)
                      .WithOne(i => i.BNPL_PLAN)
                      .HasForeignKey(i => i.Bnpl_PlanID)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related BNPL_Installments exist

                // (M) — (1) BNPL_PlanType
                entity.HasOne(p => p.BNPL_PlanType)
                      .WithMany(pt => pt.BNPL_PLANs)
                      .HasForeignKey(p => p.Bnpl_PlanTypeID)
                      .OnDelete(DeleteBehavior.Restrict); // Prevents deleting if related BNPL_PlanType exist
            });

            // -------------------------------------------------------------
            // BNPL_Installment
            // -------------------------------------------------------------
            modelBuilder.Entity<BNPL_Installment>(entity =>
            {
                entity.Property(i => i.Installment_BaseAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.OverPaymentCarriedFromPreviousInstallment)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.LateInterest)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.TotalDueAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(i => i.AmountPaid_AgainstBase)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.AmountPaid_AgainstLateInterest)
                      .HasColumnType("decimal(18,2)");

                entity.Property(i => i.RowVersion)
                    .HasColumnType("BINARY(8)")
                    .IsRequired()
                    .IsConcurrencyToken();
            });

            // -------------------------------------------------------------
            // BNPL_PlanSettlementSummary
            // -------------------------------------------------------------
            modelBuilder.Entity<BNPL_PlanSettlementSummary>(entity =>
            {
                //** [Strat : Ensures only ONE record per Bnpl_PlanID (Status = Ative(1), IsLatest = true)] **
                // Add generated column for MySQL
                entity.Property<int?>("ActiveLatestKey")
                    .HasColumnType("int")
                    .HasComputedColumnSql(
                        "IF(`Bnpl_PlanSettlementSummary_Status` = 1 AND `IsLatest` = 1, `Bnpl_PlanID`, NULL)",
                        stored: true
                    );

                // Add unique index on the generated column
                entity.HasIndex("ActiveLatestKey").IsUnique();
                //** [End : Ensures only ONE record per Bnpl_PlanID (Ative, IsLatest = true)] *****************

                entity.Property(s => s.NotYetDueCurrentInstallmentBaseAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(s => s.Total_InstallmentBaseArrears)
                      .HasColumnType("decimal(18,2)");

                entity.Property(s => s.Total_LateInterest)
                      .HasColumnType("decimal(18,2)");

                entity.Property(s => s.Total_PayableSettlement)
                    .HasColumnType("decimal(18,2)");

                entity.Property(s => s.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount) 
                    .HasColumnType("decimal(18,2)");   

                entity.Property(s => s.Paid_AgainstTotalArrears)
                      .HasColumnType("decimal(18,2)");

                entity.Property(s => s.Paid_AgainstTotalLateInterest)
                      .HasColumnType("decimal(18,2)");  

                entity.Property(s => s.Total_OverpaymentCarriedToNext)
                      .HasColumnType("decimal(18,2)");

                entity.Property(s => s.IsLatest)
                    .HasDefaultValue(true);  

                entity.Property(s => s.RowVersion)
                    .HasColumnType("BINARY(8)")
                    .IsRequired()
                    .IsConcurrencyToken();  
            });
        }
        //-------- [End: configure model] -------------



        //-------- [Start: Intercept DateTime + Auto Timestamp] -----------
        public override int SaveChanges()
        {
            ApplySriLankaTimeZone();
            ApplyTimestamps();
            ApplyRowVersion(); 
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplySriLankaTimeZone();
            ApplyTimestamps();
            ApplyRowVersion(); 
            return await base.SaveChangesAsync(cancellationToken);
        }

        //Helper method : Time Zone
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
                        if (dateTimeValue.Kind == DateTimeKind.Utc || dateTimeValue.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = TimeZoneHelper.ToSriLankaTime(dateTimeValue);
                        }
                    }
                }
            }
        }

        //Helper method : Time Stamp
        private void ApplyTimestamps()
        {
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            foreach (var entry in ChangeTracker.Entries<BaseModel>())
            {
                if (entry.State == EntityState.Added)
                {
                    // Set CreatedAt only when the entity is first added
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    // Prevent updating CreatedAt on modifications
                    entry.Property(e => e.CreatedAt).IsModified = false;
                    entry.Entity.UpdatedAt = now;
                }
            }
        }
        //-------- [End: Intercept DateTime + Auto Timestamp] -----------


        //-------- [Start: Concurrency Handle - RowVersion] -------------
        //Helper method : Concurrency
        private void ApplyRowVersion()
        {
            var entries = ChangeTracker.Entries()
                .Where(e =>
                    (e.State == EntityState.Added || e.State == EntityState.Modified) &&
                    e.Properties.Any(p => p.Metadata.Name == "RowVersion")
                );

            foreach (var entry in entries)
            {
                entry.Property("RowVersion").CurrentValue = RandomNumberGenerator.GetBytes(8);
            }
        }
        //-------- [End: Concurrency Handle - RowVersion] ---------------
    }
}