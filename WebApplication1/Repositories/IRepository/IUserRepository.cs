using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IUserRepository
    {
        //CRUD operations
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetWithRoleProfileDetailsByIdAsync(int id);
        Task AddAsync(User user);

        //Custom Query Operations
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
    }
}