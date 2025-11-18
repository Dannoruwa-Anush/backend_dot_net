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

        //CRUD operations
        public async Task AddAsync(BNPL_PlanSettlementSummary bNPL_PlanSettlementSummary)
        {
            await _context.BNPL_PlanSettlementSummaries.AddAsync(bNPL_PlanSettlementSummary);
            //SaveChangesAsync() is handled by the service layer to ensure atomic operations (Transaction handling).
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

            //SaveChangesAsync() is handled by the service layer to ensure atomic operations (Transaction handling).
        }

        public async Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotAsync(int planId)
        {
            return await _context.BNPL_PlanSettlementSummaries
                .Where(s => s.Bnpl_PlanID == planId && s.IsLatest)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }
    }
}