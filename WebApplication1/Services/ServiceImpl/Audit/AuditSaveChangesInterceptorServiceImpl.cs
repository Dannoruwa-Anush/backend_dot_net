using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApplication1.Data;
using WebApplication1.Models.Audit;
using WebApplication1.Models.Base;
using WebApplication1.Services.IService.Audit;
using WebApplication1.Services.IService.Auth;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl.Audit
{
    // Note:
    // Captures all entity changes and writes them to AuditLog.
    // Uses DbContextFactory to safely create DbContext inside a singleton interceptor.
    public sealed class AuditSaveChangesInterceptorServiceImpl : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly List<AuditEntrySnapshot> _pendingAudits = new();

        public AuditSaveChangesInterceptorServiceImpl(
            IServiceProvider serviceProvider,
            IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _serviceProvider = serviceProvider;
            _dbContextFactory = dbContextFactory;
        }

        // ===== Capture BEFORE SAVE =====
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context;
            if (context == null) return result;

            _pendingAudits.Clear();

            var entries = context.ChangeTracker.Entries()
                .Where(e =>
                    e.Entity is BaseModel &&
                    !(e.Entity is AuditLog) &&
                    (e.State == EntityState.Added ||
                     e.State == EntityState.Modified ||
                     e.State == EntityState.Deleted));

            foreach (var entry in entries)
                _pendingAudits.Add(new AuditEntrySnapshot(entry));

            return result;
        }

        // ===== AFTER SAVE SYNC =====
        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            if (_pendingAudits.Count == 0) return result;

            WriteAuditLogs();
            _pendingAudits.Clear();

            return result;
        }

        // ===== AFTER SAVE ASYNC =====
        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (_pendingAudits.Count == 0) return result;

            await WriteAuditLogsAsync(cancellationToken);
            _pendingAudits.Clear();

            return result;
        }

        // ===== SYNC WRITE =====
        private void WriteAuditLogs()
        {
            using var context = _dbContextFactory.CreateDbContext();
            Write(context);
            context.SaveChanges();
        }

        // ===== ASYNC WRITE =====
        private async Task WriteAuditLogsAsync(CancellationToken ct)
        {
            using var context = _dbContextFactory.CreateDbContext();
            Write(context);
            await context.SaveChangesAsync(ct);
        }

        // ===== CORE WRITE LOGIC =====
        private void Write(AppDbContext context)
        {
            using var scope = _serviceProvider.CreateScope();

            var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
            var requestContextService = scope.ServiceProvider.GetRequiredService<IRequestContextService>();
            var user = currentUserService.UserProfile;
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            foreach (var audit in _pendingAudits)
            {
                audit.ResolvePrimaryKey();

                context.Set<AuditLog>().Add(new AuditLog
                {
                    Action = audit.Action,
                    EntityName = audit.EntityName,
                    EntityId = audit.EntityId,

                    UserId = user?.UserID,
                    Email = user?.Email ?? AuditTrailSystemConstants.SystemEmail,
                    Role = user?.Role ?? AuditTrailSystemConstants.AnonymousRole,
                    Position = user?.EmployeePosition ?? AuditTrailSystemConstants.PublicPosition,

                    IpAddress = requestContextService.IpAddress,
                    UserAgent = requestContextService.UserAgent,

                    Changes = audit.Changes,
                    CreatedAt = now
                });
            }
        }
    }
}