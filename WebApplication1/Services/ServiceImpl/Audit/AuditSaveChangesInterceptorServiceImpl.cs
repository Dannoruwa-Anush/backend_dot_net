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
    //Note: captures all entity changes and saves them in AuditLog
    public sealed class AuditSaveChangesInterceptorServiceImpl : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRequestContextService _requestContextService;
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        private readonly List<AuditEntrySnapshot> _pendingAudits = new();

        // Constructor
        public AuditSaveChangesInterceptorServiceImpl(ICurrentUserService currentUserService, IRequestContextService requestContextService, IDbContextFactory<AppDbContext> dbContextFactory)
        {
            // Dependency injection
            _currentUserService = currentUserService;
            _requestContextService = requestContextService;
            _dbContextFactory = dbContextFactory;
        }

        // BEFORE SAVE
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
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

        // AFTER SAVE (SYNC)
        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            if (_pendingAudits.Count == 0)
                return result;

            WriteAuditLogs();
            _pendingAudits.Clear();

            return result;
        }

        // AFTER SAVE (ASYNC)
        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (_pendingAudits.Count == 0)
                return result;

            await WriteAuditLogsAsync(cancellationToken);
            _pendingAudits.Clear();

            return result;
        }


        // WRITE AUDIT LOGS (SYNC)
        private void WriteAuditLogs()
        {
            using var context = _dbContextFactory.CreateDbContext();
            Write(context);
            context.SaveChanges();
        }

        // WRITE AUDIT LOGS (ASYNC)
        private async Task WriteAuditLogsAsync(CancellationToken ct)
        {
            using var context = _dbContextFactory.CreateDbContext();
            Write(context);
            await context.SaveChangesAsync(ct);
        }

        private void Write(AppDbContext context)
        {
            var user = _currentUserService.UserProfile;
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

                    IpAddress = _requestContextService.IpAddress,
                    UserAgent = _requestContextService.UserAgent,

                    Changes = audit.Changes,
                    CreatedAt = now
                });
            }
        }
    }
}