using WebApplication1.Models;

namespace WebApplication1.Services.IService.Auth
{
    public interface IAuthService
    {
        Task<User> RegisterUserAsync(User user);
        Task<string> LoginAsync(string email, string password);
    }
}