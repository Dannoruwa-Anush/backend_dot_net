using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class BNPL_PlanSettlementSummaryRepositoryImpl : IBNPL_PlanSettlementSummaryRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public BNPL_PlanSettlementSummaryRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD Operations
        public async Task<IEnumerable<BNPL_PlanSettlementSummary>> GetAllByPlanIdAsync(int planId)
        {
            return await _context.BNPL_PlanSettlementSummaries
                .Where(s => s.Bnpl_PlanID == planId && s.IsLatest)
                .ToListAsync();
        }

        //Custom Query Operations
        public async Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotAsync(int planId)
        {
            return await _context.BNPL_PlanSettlementSummaries
                .Where(s => s.Bnpl_PlanID == planId && s.IsLatest)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId)
        {
            return await _context.BNPL_PlanSettlementSummaries
                .Include(s => s.BNPL_PLAN)
                    .ThenInclude(so => so!.CustomerOrder)
                .Where(s => s.BNPL_PLAN!.CustomerOrder!.OrderID == orderId && s.IsLatest)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task MarkPreviousSnapshotsAsNotLatestAsync(int planId)
        {
            var previousSnapshots = await _context.BNPL_PlanSettlementSummaries
                .Where(s => s.Bnpl_PlanID == planId && s.IsLatest)
                .ToListAsync();

            foreach (var snapshot in previousSnapshots)
            {
                snapshot.IsLatest = false;
            }
        }

        public async Task MarkPreviousSnapshotsAsNotLatestBatchAsync(List<int> planIds)
        {
            if (planIds == null || !planIds.Any())
                return;

            var previousSnapshots = await _context.BNPL_PlanSettlementSummaries
                .Where(s => planIds.Contains(s.Bnpl_PlanID) && s.IsLatest)
                .ToListAsync();

            foreach (var snapshot in previousSnapshots)
            {
                snapshot.IsLatest = false;
            }
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_PlanSettlementSummary>> GetAllLatestSnapshotWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            var query = _context.BNPL_PlanSettlementSummaries
                        .Include(s => s.BNPL_PLAN)
                            .ThenInclude(so => so!.CustomerOrder)
                            .ThenInclude(sc => sc!.Customer)
                        .Where(s => s.IsLatest)
                        .AsNoTracking()
                        .AsQueryable();

            // Apply filters from helper
            query = ApplySearch(query, searchKey);

            query = query.OrderByDescending(c => c.CreatedAt);

            // Total count after filters
            var totalCount = await query.CountAsync();

            // Pagination
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return paginated result
            return new PaginationResultDto<BNPL_PlanSettlementSummary>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Helper method: Search filter (email, phone, order date)
        private IQueryable<BNPL_PlanSettlementSummary> ApplySearch(IQueryable<BNPL_PlanSettlementSummary> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();

                query = query.Where(p =>
                    p.BNPL_PLAN!.CustomerOrder!.OrderID.ToString().Contains(searchKey) ||
                    p.BNPL_PLAN!.BNPL_PlanType!.Bnpl_PlanTypeID.ToString().Contains(searchKey) ||
                    (p.BNPL_PLAN!.CustomerOrder!.Customer!.User.Email != null && p.BNPL_PLAN!.CustomerOrder!.Customer.User.Email.ToLower().Contains(searchKey)) ||
                    (p.BNPL_PLAN!.CustomerOrder!.Customer.PhoneNo != null && p.BNPL_PLAN!.CustomerOrder!.Customer.PhoneNo.ToLower().Contains(searchKey))
                );
            }
            return query;
        }
    }
}