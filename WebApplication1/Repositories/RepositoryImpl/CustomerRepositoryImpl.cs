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

        //CRUD operations
        public async Task<IEnumerable<Customer>> GetAllAsync() =>
            await _context.Customers
                    .Include(cu => cu.User)
                    .ToListAsync();

        public async Task<Customer?> GetByIdAsync(int id) =>
            await _context.Customers
                    .Include(cu => cu.User)
                    .FirstOrDefaultAsync(cu => cu.CustomerID == id);

        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task<Customer?> UpdateAsync(int id, Customer customer)
        {
            var existing = await _context.Customers.FindAsync(id);
            if (existing == null) 
                return null;

            existing.CustomerName = customer.CustomerName;
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

        //Custom Query Operations
        public async Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            var query = _context.Customers.AsQueryable();

            // Apply filters from helper
            query = ApplyCustomerFilters(query, searchKey);

            // Get total count after filtering
            var totalCount = await _context.Customers.CountAsync();

            // Get paginated data
            var items = await _context.Customers
                .Include(cu => cu.User)
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

        //Helper method: filter by customer phone no
        private IQueryable<Customer> ApplyCustomerFilters(IQueryable<Customer> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim();
                query = query.Where(cu => EF.Functions.Like(cu.PhoneNo, $"%{searchKey}%"));
            }

            return query;
        }
        
        public async Task<bool> ExistsByPhoneNoAsync(string phoneNo)
        {
            return await _context.Customers.AnyAsync(cu => cu.PhoneNo == phoneNo);
        }

        public async Task<bool> ExistsByPhoneNoAsync(string phoneNo, int excludeId)
        {
            return await _context.Customers.AnyAsync(cu => cu.PhoneNo == phoneNo && cu.CustomerID != excludeId);
        }
    }
}