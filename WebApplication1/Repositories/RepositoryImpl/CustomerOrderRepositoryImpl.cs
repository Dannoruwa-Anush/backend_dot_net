using Microsoft.EntityFrameworkCore;
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
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD Operations
        public async Task<IEnumerable<CustomerOrder>> GetAllAsync() =>
            await _context.CustomerOrders.ToListAsync();

        public async Task<IEnumerable<CustomerOrder>> GetAllWithCustomerDetailsAsync() =>
            await _context.CustomerOrders
                .Include(o => o.Customer)
                    .ThenInclude(ou => ou!.User)
                .ToListAsync();

        public async Task<CustomerOrder?> GetByIdAsync(int id) =>
            await _context.CustomerOrders.FindAsync(id);

        public async Task<CustomerOrder?> GetWithCustomerOrderDetailsByIdAsync(int id) =>
            await _context.CustomerOrders
                .Include(o => o.Customer)
                    .ThenInclude(ou => ou!.User)
                .Include(o => o.CustomerOrderElectronicItems)
                    .ThenInclude(oi => oi.ElectronicItem)
                .Include(o => o.BNPL_PLAN!)
                .FirstOrDefaultAsync(o => o.OrderID == id);

        public async Task<CustomerOrder?> GetWithFinancialDetailsByIdAsync(int id)
        {
            return await _context.CustomerOrders
                .Include(o => o.CustomerOrderElectronicItems)
                    .ThenInclude(oi => oi.ElectronicItem)
                .Include(o => o.BNPL_PLAN!)
                    .ThenInclude(p => p.BNPL_Installments)
                .Include(o => o.BNPL_PLAN!)
                    .ThenInclude(p => p.BNPL_PlanSettlementSummaries)
                .FirstOrDefaultAsync(o => o.OrderID == id);
        }

        public async Task<CustomerOrder?> GetWithCustomerFinancialDetailsByIdAsync(int id)
        {
            return await _context.CustomerOrders
                .Include(o => o.Customer)
                    .ThenInclude(ou => ou!.User)
                .Include(o => o.CustomerOrderElectronicItems)
                    .ThenInclude(oi => oi.ElectronicItem)
                .Include(o => o.BNPL_PLAN!)
                    .ThenInclude(p => p.BNPL_Installments)
                .Include(o => o.BNPL_PLAN!)
                    .ThenInclude(p => p.BNPL_PlanSettlementSummaries)
                .FirstOrDefaultAsync(o => o.OrderID == id);
        }

        public async Task AddAsync(CustomerOrder customerOrder) =>
            await _context.CustomerOrders.AddAsync(customerOrder);

        public async Task<CustomerOrder?> UpdateAsync(int id, CustomerOrder updatedOrder)
        {
            var existing = await _context.CustomerOrders.FindAsync(id);
            if (existing == null)
                return null;

            existing.OrderStatus = updatedOrder.OrderStatus;
            existing.OrderPaymentStatus = updatedOrder.OrderPaymentStatus;

            existing.ShippedDate = updatedOrder.ShippedDate;
            existing.DeliveredDate = updatedOrder.DeliveredDate;
            existing.CancelledDate = updatedOrder.CancelledDate;
            existing.PaymentCompletedDate = updatedOrder.PaymentCompletedDate;

            _context.CustomerOrders.Update(existing);
            return existing;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<CustomerOrder>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null)
        {
            // Start query
            var query = _context.CustomerOrders
                .Include(o => o.Customer)
                    .ThenInclude(ou => ou!.User)
                .Include(o => o.CustomerOrderElectronicItems)
                    .ThenInclude(oi => oi.ElectronicItem)
                .AsQueryable();

            // Apply filters
            query = ApplyPaymentStatusFilter(query, paymentStatusId);
            query = ApplyOrderStatusFilter(query, orderStatusId);
            query = ApplySearch(query, searchKey);

            query = query.OrderByDescending(c => c.OrderDate);

            // Total count after filters
            var totalCount = await query.CountAsync();

            // Pagination
            var items = await query
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
            if (string.IsNullOrWhiteSpace(searchKey))
                return query;

            searchKey = searchKey.Trim().ToLower();

            // Try detect date search
            bool isDateSearch = DateTime.TryParse(searchKey, out var parsedDate);

            return query.Where(o =>
                // Email search
                (o.Customer!.User.Email != null &&
                 o.Customer.User.Email.ToLower().Contains(searchKey))

                // Phone search
                || (o.Customer.PhoneNo != null &&
                    o.Customer.PhoneNo.ToLower().Contains(searchKey))

                // Date search (EF Core safe)
                || (isDateSearch && o.OrderDate.Date == parsedDate.Date)
            );
        }

        public async Task<bool> ExistsByCustomerAsync(int customerId)
        {
            return await _context.CustomerOrders.AnyAsync(o => o.CustomerID == customerId);
        }

        public async Task<bool> ExistsPendingOrderForCustomerAsync(int customerId)
        {
            return await _context.CustomerOrders.AnyAsync(o =>
                o.CustomerID == customerId &&
                o.OrderStatus == OrderStatusEnum.Pending &&
                o.OrderPaymentStatus == OrderPaymentStatusEnum.Awaiting_Payment
            );
        }

        public async Task<PaginationResultDto<CustomerOrder>> GetAllByCustomerWithPaginationAsync(int customerId, int pageNumber, int pageSize, int? orderStatusId = null, string? searchKey = null)
        {
            // Start query
            var query = _context.CustomerOrders
                .Include(o => o.Customer) // Include customer for email/phone search
                .AsQueryable();

            //filter by customer Id
            query = query.Where(o => o.CustomerID == customerId);

            // Apply filters
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

        public async Task<IEnumerable<CustomerOrder>> GetAllPaymentPendingByPhysicalShopSessionIdAsync(int physicalShopSessionId)
        {
            return await _context.CustomerOrders
                .AsNoTracking()
                .Where(co =>
                    co.PhysicalShopSessionId == physicalShopSessionId &&
                    co.OrderSource == OrderSourceEnum.PhysicalShop &&
                    co.OrderPaymentStatus == OrderPaymentStatusEnum.Awaiting_Payment &&
                    co.OrderStatus != OrderStatusEnum.Cancelled
                )
                .OrderBy(co => co.OrderDate)
                .ToListAsync();
        }

        public async Task<CustomerOrder?> GetActiveBnplByIdAsync(int id, int? customerId = null)
        {
            var query = _context.CustomerOrders
                .Include(o => o.CustomerOrderElectronicItems)
                    .ThenInclude(oi => oi.ElectronicItem)
                .Include(o => o.Customer)
                .Include(o => o.BNPL_PLAN)
                .Where(o => o.OrderID == id
                            && o.OrderPaymentMode == OrderPaymentModeEnum.Pay_Bnpl
                            && o.BNPL_PLAN != null
                            && o.BNPL_PLAN.Bnpl_Status == BnplStatusEnum.Active);

            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerID == customerId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<CustomerOrder>> GetAllActiveBnplByCustomerIdAsync(int customerId)
        {
            return await _context.CustomerOrders
                .AsNoTracking()
                .Include(o => o.CustomerOrderElectronicItems)
                    .ThenInclude(oi => oi.ElectronicItem)
                .Include(o => o.Customer)
                .Include(o => o.BNPL_PLAN)
                .Where(o =>
                    o.CustomerID == customerId &&
                    o.OrderPaymentMode == OrderPaymentModeEnum.Pay_Bnpl &&
                    o.BNPL_PLAN != null &&
                    o.BNPL_PLAN.Bnpl_Status == BnplStatusEnum.Active)
                .ToListAsync();
        }

        public async Task<List<CustomerOrder>> GetExpiredPendingOnlineOrdersAsync(DateTime now)
        {
            return await _context.CustomerOrders
                .Where(o =>
                    o.OrderSource == OrderSourceEnum.OnlineShop &&
                    o.OrderStatus == OrderStatusEnum.Pending &&
                    o.OrderPaymentStatus == OrderPaymentStatusEnum.Awaiting_Payment &&
                    o.PendingPaymentOrderAutoCancelledDate != null &&
                    o.PendingPaymentOrderAutoCancelledDate <= now)
                .Include(o => o.CustomerOrderElectronicItems)
                .ToListAsync();
        }
    }
}