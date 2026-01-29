
using WebApplication1.Services.IService.Audit;

namespace WebApplication1.Services.ServiceImpl.Audit
{
    public class RequestContextServiceImpl : IRequestContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor
        public RequestContextServiceImpl(IHttpContextAccessor httpContextAccessor)
        {
            // Dependency injection
            _httpContextAccessor = httpContextAccessor;
        }

        public string? IpAddress =>
            _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
            ?? _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        public string? UserAgent =>
            _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}