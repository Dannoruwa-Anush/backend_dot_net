using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class CustomerRepositoryImpl : ICustomerRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public CustomerRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync() =>
            await _context.Customers.ToListAsync();

        public async Task<Customer?> GetByIdAsync(int id) =>
            await _context.Customers.FindAsync(id);

        public async Task AddAsync(Customer customer) =>
            await _context.Customers.AddAsync(customer);


        public async Task<Customer?> UpdateAsync(int id, Customer customer)
        {
            var existing = await _context.Customers.FindAsync(id);
            if (existing == null) 
                return null;

            existing.CustomerName = customer.CustomerName;
            existing.Email = customer.Email;
            existing.PhoneNo = customer.PhoneNo;
            existing.Address = customer.Address;

            _context.Customers.Update(existing);
            await _context.SaveChangesAsync();

            return existing; 
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) 
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.Customers.CountAsync();

            var items = await _context.Customers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<Customer>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Customers.AnyAsync(cu => cu.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> ExistsByEmailAsync(string email, int excludeId)
        {
            return await _context.Customers.AnyAsync(cu => cu.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && cu.CustomerID != excludeId);
        }
    }
}