using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Project_Enums;

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
            await _context.CustomerOrders
                .Include(o => o.Customer)
                    .ThenInclude(ou => ou.User)
                .Include(o => o.CustomerOrderElectronicItems)
                    .ThenInclude(oi => oi.ElectronicItem)
                .FirstOrDefaultAsync(o => o.OrderID == id);

        public async Task AddAsync(CustomerOrder customerOrder)
        {
            await _context.CustomerOrders.AddAsync(customerOrder);
            //SaveChangesAsync() is handled by the service layer to ensure atomic operations (Transaction handling).
        }

        public async Task<CustomerOrder?> UpdateAsync(int id, CustomerOrder customerOrder)
        {
            var existing = await _context.CustomerOrders.FindAsync(id);
            if (existing == null)
                return null;

            existing.OrderPaymentStatus = customerOrder.OrderPaymentStatus;
            existing.OrderStatus = customerOrder.OrderStatus;

            _context.CustomerOrders.Update(existing);
            //SaveChangesAsync() is handled by the service layer to ensure atomic operations (Transaction handling).

            return existing;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null)
        {
            // Start query
            var query = _context.CustomerOrders
                .Include(o => o.Customer) // Include customer for email/phone search
                .AsQueryable();

            // Apply filters
            query = ApplyPaymentStatusFilter(query, paymentStatusId);
            query = ApplyOrderStatusFilter(query, orderStatusId);
            query = ApplySearch(query, searchKey);

            query = query.OrderByDescending(c => c.CreatedAt);

            // Total count after filters
            var totalCount = await query.CountAsync();

            // Pagination
            var items = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<CustomerOrder>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Helper method: Payment status filter
        private IQueryable<CustomerOrder> ApplyPaymentStatusFilter(IQueryable<CustomerOrder> query, int? paymentStatusId)
        {
            if (paymentStatusId.HasValue)
            {
                var status = (OrderPaymentStatusEnum)paymentStatusId.Value;
                query = query.Where(o => o.OrderPaymentStatus == status);
            }
            return query;
        }

        // Helper method: Order status filter
        private IQueryable<CustomerOrder> ApplyOrderStatusFilter(IQueryable<CustomerOrder> query, int? orderStatusId)
        {
            if (orderStatusId.HasValue)
            {
                var status = (OrderStatusEnum)orderStatusId.Value;
                query = query.Where(o => o.OrderStatus == status);
            }
            return query;
        }

        // Helper method: Search filter (email, phone, order date)
        private IQueryable<CustomerOrder> ApplySearch(IQueryable<CustomerOrder> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();
                query = query.Where(o =>
                    (o.Customer.User.Email != null && o.Customer.User.Email.ToLower().Contains(searchKey)) ||
                    (o.Customer.PhoneNo != null && o.Customer.PhoneNo.ToLower().Contains(searchKey)) ||
                    o.OrderDate.ToString("yyyy-MM-dd").Contains(searchKey)
                );
            }
            return query;
        }

        public async Task<bool> ExistsByCustomerAsync(int customerId)
        {
            return await _context.CustomerOrders.AnyAsync(o => o.CustomerID == customerId);
        }

        // EF transaction support
        public async Task<IDbContextTransaction> BeginTransactionAsync() =>
            await _context.Database.BeginTransactionAsync();

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}