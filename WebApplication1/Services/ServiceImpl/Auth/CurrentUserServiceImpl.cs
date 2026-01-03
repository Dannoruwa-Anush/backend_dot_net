using System.Security.Claims;
using WebApplication1.Services.IService.Auth;

namespace WebApplication1.Services.ServiceImpl.Auth
{
    public class CurrentUserServiceImpl : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor
        public CurrentUserServiceImpl(IHttpContextAccessor httpContextAccessor)
        {
            // Dependency injection
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public int? UserID
        {
            get
            {
                var claim = User?.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
            }
        }

        public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

        public string? EmployeePosition => User?.FindFirst("EmployeePosition")?.Value;
    }
}