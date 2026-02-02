using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class InvoiceRepositoryImpl : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public InvoiceRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<Invoice>> GetAllAsync() =>
            await _context.Invoices.ToListAsync();

        public async Task<Invoice?> GetByIdAsync(int id) =>
            await _context.Invoices.FindAsync(id);

        public async Task<Invoice?> GetInvoiceWithOrderAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.CustomerOrder)
                    .ThenInclude(o => o!.Customer)
                .Include(i => i.CustomerOrder)
                    .ThenInclude(b => b!.BNPL_PLAN)
                .Include(i => i.Cashflows)
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceId);
        }

        public async Task<Invoice?> GetInvoiceWithOrderFinancialDetailsAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.CustomerOrder)
                    .ThenInclude(o => o!.Customer)
                .Include(i => i.CustomerOrder)
                    .ThenInclude(o => o!.BNPL_PLAN)
                        .ThenInclude(p => p!.BNPL_PlanSettlementSummaries)
                .Include(i => i.CustomerOrder)
                    .ThenInclude(o => o!.BNPL_PLAN)
                        .ThenInclude(p => p!.BNPL_Installments)
                .Include(i => i.Cashflows)

                .AsSplitQuery()
                .FirstOrDefaultAsync(i => i.InvoiceID == invoiceId);
        }

        public async Task<Invoice?> UpdateAsync(int id, Invoice invoice)
        {
            var existing = await _context.Invoices.FindAsync(id);
            if (existing == null)
                return null;

            existing.InvoiceStatus = invoice.InvoiceStatus;
            _context.Invoices.Update(existing);
            return existing;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, int? customerId = null, int? orderSourceId = null, string? searchKey = null)
        {
            // Start query with necessary includes
            var query = _context.Invoices
                .Include(i => i.CustomerOrder)
                    .ThenInclude(co => co!.CustomerOrderElectronicItems)
                .Include(i => i.CustomerOrder)
                    .ThenInclude(co => co!.BNPL_PLAN)
                .Include(i => i.CustomerOrder)
                    .ThenInclude(co => co!.Customer)
                        .ThenInclude(c => c!.User)
                
                // Only Paid / Refund cashflows
                .Include(i => i.Cashflows
                    .Where(c =>
                        c.CashflowPaymentNature == CashflowPaymentNatureEnum.Payment ||
                        c.CashflowPaymentNature == CashflowPaymentNatureEnum.Refund))    
                .AsQueryable();

            // Apply filters
            query = ApplyInvoiceTypeFilter(query, invoiceTypeId);
            query = ApplyInvoiceStatusFilter(query, invoiceStatusId);
            query = ApplyCustomerFilter(query, customerId);
            query = ApplyOrderSourceFilter(query, orderSourceId);
            query = ApplySearch(query, searchKey);

            // Order by creation date (descending)
            query = query.OrderByDescending(i => i.CreatedAt);

            // Total count after filters
            var totalCount = await query.CountAsync();

            // Pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<Invoice>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Helper method: Invoice Type filter
        private IQueryable<Invoice> ApplyInvoiceTypeFilter(IQueryable<Invoice> query, int? invoiceTypeId)
        {
            if (invoiceTypeId.HasValue)
            {
                var type = (InvoiceTypeEnum)invoiceTypeId.Value;
                query = query.Where(i => i.InvoiceType == type);
            }
            return query;
        }

        // Helper method: Invoice Status filter
        private IQueryable<Invoice> ApplyInvoiceStatusFilter(IQueryable<Invoice> query, int? invoiceStatusId)
        {
            if (invoiceStatusId.HasValue)
            {
                var status = (InvoiceStatusEnum)invoiceStatusId.Value;
                query = query.Where(i => i.InvoiceStatus == status);
            }
            return query;
        }

        // Helper method: Customer filter
        private IQueryable<Invoice> ApplyCustomerFilter(IQueryable<Invoice> query, int? customerId)
        {
            if (customerId.HasValue)
            {
                query = query.Where(i =>
                    i.CustomerOrder != null &&
                    i.CustomerOrder.CustomerID == customerId.Value
                );
            }
            return query;
        }

        // Helper method: Order Source Filter
        private IQueryable<Invoice> ApplyOrderSourceFilter(IQueryable<Invoice> query, int? orderSourceId)
        {
            if (orderSourceId.HasValue &&
                Enum.IsDefined(typeof(OrderSourceEnum), orderSourceId.Value))
            {
                var source = (OrderSourceEnum)orderSourceId.Value;

                query = query.Where(i =>
                    i.CustomerOrder != null &&
                    i.CustomerOrder.OrderSource == source
                );
            }

            return query;
        }

        // Helper method: Search filter (email, phone, order date)
        private IQueryable<Invoice> ApplySearch(IQueryable<Invoice> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim();

                query = query.Where(i =>
                    i.CustomerOrder != null &&
                    (
                        (i.CustomerOrder.Customer != null &&
                         i.CustomerOrder.Customer.User != null &&
                         i.CustomerOrder.Customer.User.Email != null &&
                         EF.Functions.Like(i.CustomerOrder.Customer.User.Email, $"%{searchKey}%")) ||

                        (i.CustomerOrder.Customer != null &&
                         i.CustomerOrder.Customer.PhoneNo != null &&
                         EF.Functions.Like(i.CustomerOrder.Customer.PhoneNo, $"%{searchKey}%")) ||

                        EF.Functions.Like(
                            i.CustomerOrder.OrderDate.ToString("yyyy-MM-dd"),
                            $"%{searchKey}%"
                        )
                    )
                );
            }
            return query;
        }

        public async Task<bool> ExistsUnpaidInvoiceByCustomerAsync(int customerId)
        {
            return await _context.Invoices
                .AsNoTracking()
                .AnyAsync(i =>
                    i.InvoiceStatus == InvoiceStatusEnum.Unpaid &&
                    i.CustomerOrder != null &&
                    i.CustomerOrder.CustomerID == customerId
                );
        }

        public async Task<Invoice?> GetLatestUnpaidInstallmentInvoiceByOrderIdAsync(int orderId)
        {
            return await _context.Invoices
                .Where(i =>
                    i.OrderID == orderId &&
                    i.InvoiceStatus == InvoiceStatusEnum.Unpaid &&
                    i.InvoiceType == InvoiceTypeEnum.Bnpl_Installment_Pay)
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}