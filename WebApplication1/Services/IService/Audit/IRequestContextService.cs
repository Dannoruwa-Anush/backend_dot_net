namespace WebApplication1.Services.IService.Audit
{
    public interface IRequestContextService
    {
        string? IpAddress { get; }
        string? UserAgent { get; } // what kind of client is making the request, including: Browser or app name, Version, Operating system, Device type
    }
}