using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class FileServiceImpl : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileServiceImpl(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Invalid file.");

            string uploadFolder = Path.Combine(_env.WebRootPath, folder);
            Directory.CreateDirectory(uploadFolder);

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Path.Combine(folder, fileName).Replace("\\", "/");
        }

        public bool DeleteFile(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return false;

            string fullPath = Path.Combine(_env.WebRootPath, relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }

            return false;
        }

        public string BuildFileUrl(HttpRequest request, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            return $"{request.Scheme}://{request.Host}/{relativePath}";
        }
    }
}