namespace WebApplication1.Services.IService
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        bool DeleteFile(string relativePath);
        string BuildFileUrl(HttpRequest request, string relativePath);
    }
}