using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class CustomerOrderRepositoryImpl : ICustomerOrderRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public CustomerOrderRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        //CRUD Operations
        public async Task<IEnumerable<CustomerOrder>> GetAllAsync() =>
            await _context.CustomerOrders.ToListAsync();

        public async Task<CustomerOrder?> GetByIdAsync(int id) =>
            await _context.CustomerOrders.FindAsync(id);

        public async Task AddAsync(CustomerOrder customerOrder)
        {
            await _context.CustomerOrders.AddAsync(customerOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<CustomerOrder?> UpdateAsync(int id, CustomerOrder customerOrder)
        {
            var existing = await _context.CustomerOrders.FindAsync(id);
            if (existing == null) 
                return null;

            existing.PaymentStatus = customerOrder.PaymentStatus;

            _context.CustomerOrders.Update(existing);
            await _context.SaveChangesAsync();

            return existing; 
        }
    }
}