using WebApplication1.Models;

namespace WebApplication1.Services.IService.Auth
{
    public interface IAuthService
    {
        //Single Repository Operations (save immediately)
        Task<User> RegisterUserWithSaveAsync(User user);

        Task<(User user, string token)> LoginAsync(string email, string password);
    }
}