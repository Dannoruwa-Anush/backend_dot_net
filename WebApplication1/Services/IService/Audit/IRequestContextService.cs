using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService.Audit
{
    public interface IRequestContextService
    {
        string? IpAddress { get; }
        string? UserAgent { get; }
    }
}