using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService.Audit
{
    public interface IAuditLogService
    {
        void LogEntityAction(AuditActionTypeEnum action, string entityName, int entityId, string entityDisplayName);
    }
}