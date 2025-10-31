using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class CustomerOrderElectronicItemRepositoryImpl : ICustomerOrderElectronicItemRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public CustomerOrderElectronicItemRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        //CRUD operations
        public async Task AddAsync(CustomerOrderElectronicItem orderItem) =>
            await _context.CustomerOrderElectronicItems.AddAsync(orderItem);
    }
}