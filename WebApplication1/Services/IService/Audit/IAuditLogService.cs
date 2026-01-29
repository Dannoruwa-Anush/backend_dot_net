using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService.Audit
{
    //Note: this is for - Manual events not tied to EF (Ex: a login attempt, invoice download)
    //Note: automatic audit logging in AppDbContext - Captures all entity changes (Added, Modified, Deleted) automatically
    public interface IAuditLogService
    {
        void LogEntityAction(AuditActionTypeEnum action, string entityName, int entityId, string entityDisplayName);
    }
}