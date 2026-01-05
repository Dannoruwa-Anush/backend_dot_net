using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class UserRepositoryImpl : IUserRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public UserRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task AddAsync(User user) =>
            await _context.Users.AddAsync(user);

        //Custom Query Operations
        public async Task<bool> EmailExistsAsync(string email) =>
            await _context.Users.AnyAsync(u => u.Email == email);


        public async Task<User?> GetByEmailAsync(string email) =>
            await _context.Users
            .Include(u => u.Customer)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == email);
    }
}