using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class CashflowRepositoryImpl : ICashflowRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public CashflowRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW
        
        //CRUD operations
        public async Task<IEnumerable<Cashflow>> GetAllAsync() =>
            await _context.Cashflows.ToListAsync();

        public async Task<Cashflow?> GetByIdAsync(int id) =>
            await _context.Cashflows.FindAsync(id);

        public async Task AddAsync(Cashflow cashflow) =>
            await _context.Cashflows.AddAsync(cashflow);

        //Custom Query Operations
        public async Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null)
        {
            // Start query
            var query = _context.Cashflows
                .Include(cf => cf.CustomerOrder) // include for OrderID search
                .AsQueryable();

            // Apply filters
            query = ApplyCashflowStatusFilter(query, cashflowStatusId);
            query = ApplyCashflowSearch(query, searchKey);

            query = query.OrderByDescending(cf => cf.CashflowDate);

            // Total count after filtering
            var totalCount = await query.CountAsync();

            // Pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginationResultDto<Cashflow>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Helper: filter by cashflow status
        private IQueryable<Cashflow> ApplyCashflowStatusFilter(IQueryable<Cashflow> query, int? cashflowStatusId)
        {
            if (cashflowStatusId.HasValue)
            {
                var status = (CashflowStatusEnum)cashflowStatusId.Value;
                query = query.Where(cf => cf.CashflowStatus == status);
            }
            return query;
        }

        // Helper: search by OrderID or CreatedAt
        private IQueryable<Cashflow> ApplyCashflowSearch(IQueryable<Cashflow> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();

                query = query.Where(cf =>
                    cf.OrderID.ToString().Contains(searchKey) ||
                    cf.CreatedAt.ToString("yyyy-MM-dd").Contains(searchKey)
                );
            }
            return query;
        }

       public async Task<bool> ExistsByCashflowRefAsync(string cashflowRef)
        {
            return await _context.Cashflows
                .AnyAsync(cf => cf.CashflowRef.ToLower() == cashflowRef.ToLower());
        }

        public async Task<decimal> SumCashflowsByOrderAsync(int orderId)
        {
            return await _context.Cashflows
                .Where(cf => cf.OrderID == orderId && cf.CashflowStatus == CashflowStatusEnum.Paid)
                .SumAsync(cf => cf.AmountPaid);
        }
    }
}