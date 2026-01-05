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

        //Custom Query Operations
        public async Task<PaginationResultDto<Invoice>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? invoiceTypeId = null, int? invoiceStatusId = null, string? searchKey = null)
        {
            // Start query
            var query = _context.Invoices
                .Include(i => i.CustomerOrder)
                    .ThenInclude(ie => ie!.CustomerOrderElectronicItems)
                .Include(i => i.CustomerOrder)
                    .ThenInclude(ib => ib!.BNPL_PLAN)
                .AsQueryable();

            // Apply filters
            query = ApplyInvoiceTypeFilter(query, invoiceTypeId);
            query = ApplyInvoiceStatusFilter(query, invoiceStatusId);
            query = ApplySearch(query, searchKey);

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
                var status = (InvoiceTypeEnum)invoiceTypeId.Value;
                query = query.Where(i => i.InvoiceType == status);
            }
            return query;
        }

        // Helper method: Invoice StatusFilter filter
        private IQueryable<Invoice> ApplyInvoiceStatusFilter(IQueryable<Invoice> query, int? invoiceStatusId)
        {
            if (invoiceStatusId.HasValue)
            {
                var status = (InvoiceStatusEnum)invoiceStatusId.Value;
                query = query.Where(i => i.InvoiceStatus == status);
            }
            return query;
        }

        // Helper method: Search filter (email, phone, order date)
        private IQueryable<Invoice> ApplySearch(IQueryable<Invoice> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();
                query = query.Where(i =>
                    (i.CustomerOrder!.Customer!.User.Email != null && i.CustomerOrder!.Customer.User.Email.ToLower().Contains(searchKey)) ||
                    (i.CustomerOrder!.Customer.PhoneNo != null && i.CustomerOrder!.Customer.PhoneNo.ToLower().Contains(searchKey)) ||
                    i.CustomerOrder!.OrderDate.ToString("yyyy-MM-dd").Contains(searchKey)
                );
            }
            return query;
        }
    }
}