using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

        //CRUD operations
        public async Task<IEnumerable<Cashflow>> GetAllAsync() =>
            await _context.Cashflows.ToListAsync();

        public async Task<Cashflow?> GetByIdAsync(int id) =>
            await _context.Cashflows.FindAsync(id);

        public async Task AddAsync(Cashflow cashflow)
        {
            await _context.Cashflows.AddAsync(cashflow);
            //SaveChangesAsync() is handled by the service layer to ensure atomic operations (Transaction handling).
        }

        public async Task<Cashflow?> UpdateAsync(int id, Cashflow cashflow)
        {
            var existing = await _context.Cashflows.FindAsync(id);
            if (existing == null)
                return null;

            existing.CashflowStatus = cashflow.CashflowStatus;

            _context.Cashflows.Update(existing);
            //SaveChangesAsync() is handled by the service layer to ensure atomic operations (Transaction handling).

            return existing;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null)
        {
            // Start query
            var query = _context.Cashflows
                .Include(t => t.CustomerOrder) // include for OrderID search
                .AsQueryable();

            // Apply filters
            query = ApplyCashflowStatusFilter(query, cashflowStatusId);
            query = ApplyCashflowSearch(query, searchKey);

            // Total count after filtering
            var totalCount = await query.CountAsync();

            // Pagination
            var items = await query
                .OrderByDescending(t => t.CashflowDate)
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
                query = query.Where(t => t.CashflowStatus == status);
            }
            return query;
        }

        // Helper: search by OrderID or CreatedAt
        private IQueryable<Cashflow> ApplyCashflowSearch(IQueryable<Cashflow> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();

                query = query.Where(t =>
                    t.OrderID.ToString().Contains(searchKey) ||
                    t.CreatedAt.ToString("yyyy-MM-dd").Contains(searchKey)
                );
            }
            return query;
        }

        public async Task<decimal> SumCashflowsByOrderAsync(int orderId)
        {
            return await _context.Cashflows
                .Where(x => x.OrderID == orderId && x.CashflowStatus == CashflowStatusEnum.Paid)
                .SumAsync(x => x.AmountPaid);
        }

        // EF transaction support
        public async Task<IDbContextTransaction> BeginTransactionAsync() =>
            await _context.Database.BeginTransactionAsync();

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}