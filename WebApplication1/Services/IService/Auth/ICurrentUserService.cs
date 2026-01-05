using System.Security.Claims;
using WebApplication1.Utils.Records;

namespace WebApplication1.Services.IService.Auth
{
    public interface ICurrentUserService
    {
        int? UserID { get; }
        string? Email { get; }
        string? Role { get; }
        string? EmployeePosition { get; }
        ClaimsPrincipal? User { get; }
        CurrentUserProfile UserProfile { get; }
    }
}