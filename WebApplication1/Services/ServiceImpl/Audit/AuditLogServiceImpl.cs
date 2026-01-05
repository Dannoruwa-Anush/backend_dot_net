using WebApplication1.Services.IService.Audit;
using WebApplication1.Services.IService.Auth;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl.Audit
{
    public class AuditLogServiceImpl : IAuditLogService
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuditLogServiceImpl> _logger;

        // Constructor
        public AuditLogServiceImpl(ICurrentUserService currentUserService, ILogger<AuditLogServiceImpl> logger)
        {
            // Dependency injection
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public void LogEntityAction(AuditActionTypeEnum action, string entityName, int entityId, string entityDisplayName)
        {
            var user = _currentUserService.UserProfile;

            _logger.LogInformation(
                "{Action} {Entity}: Id={EntityId}, Name={EntityName} | UserID={UserId}, Email={Email}, Role={Role}, Position={Position}",
                action,
                entityName,
                entityId,
                entityDisplayName,
                user.UserID,
                user.Email,
                user.Role,
                user.EmployeePosition
            );
        }
    }
}
