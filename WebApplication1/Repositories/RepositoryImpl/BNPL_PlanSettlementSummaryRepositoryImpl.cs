using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
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

        //CRUD operations
        public async Task AddAsync(BNPL_PlanSettlementSummary bNPL_PlanSettlementSummary) =>
            await _context.BNPL_PlanSettlementSummaries.AddAsync(bNPL_PlanSettlementSummary);

        public async Task<BNPL_PlanSettlementSummary?> UpdateAsync(int id, BNPL_PlanSettlementSummary bNPL_PlanSettlementSummary)
        {
            var existing = await _context.BNPL_PlanSettlementSummaries.FindAsync(id);
            if (existing == null)
                return null;

            existing.Bnpl_PlanSettlementSummary_Status = bNPL_PlanSettlementSummary.Bnpl_PlanSettlementSummary_Status;
            existing.IsLatest = bNPL_PlanSettlementSummary.IsLatest;

            _context.BNPL_PlanSettlementSummaries.Update(existing);
            return existing;
        }

        //Custom Query Operations
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
    }
}