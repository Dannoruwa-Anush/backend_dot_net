using System.Security.Claims;

namespace WebApplication1.Services.IService.Auth
{
    public interface ICurrentUserService
    {
        int? UserID { get; }
        string? Email { get; }
        string? Role { get; }
        string? EmployeePosition { get; }
        ClaimsPrincipal? User { get; }
    }
}