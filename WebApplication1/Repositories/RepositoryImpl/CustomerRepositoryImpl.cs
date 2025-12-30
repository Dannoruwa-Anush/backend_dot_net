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
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<Customer>> GetAllAsync() =>
            await _context.Customers.ToListAsync();

        public async Task<IEnumerable<Customer>> GetAllWithUserDeailsAsync() =>
            await _context.Customers
                    .Include(cu => cu.User)
                    .ToListAsync();

        public async Task<Customer?> GetByIdAsync(int id) =>
            await _context.Customers.FindAsync(id);

        public async Task<Customer?> GetWithUserDetailsByIdAsync(int id) =>
            await _context.Customers
                    .Include(cu => cu.User)
                    .FirstOrDefaultAsync(cu => cu.CustomerID == id);

        public async Task AddAsync(Customer customer) =>
            await _context.Customers.AddAsync(customer);

        public async Task<Customer?> UpdateProfileAsync(int id, Customer customer)
        {
            var existing = await _context.Customers.FindAsync(id);
            if (existing == null)
                return null;

            existing.CustomerName = customer.CustomerName;
            existing.PhoneNo = customer.PhoneNo;
            existing.Address = customer.Address;

            _context.Customers.Update(existing);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            _context.Customers.Remove(customer);
            return true;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Customer>> GetAllWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            var query = _context.Customers.AsNoTracking().AsQueryable();

            // Apply filters from helper
            query = ApplyCustomerFilters(query, searchKey).OrderByDescending(c => c.CreatedAt);

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

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            return await _context.Set<Customer>()
                 .Include(e => e.User)
                 .FirstOrDefaultAsync(e => e.UserID == userId);
        }
    }
}