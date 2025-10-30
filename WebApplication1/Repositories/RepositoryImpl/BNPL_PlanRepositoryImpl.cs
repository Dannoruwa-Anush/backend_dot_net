using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class BNPL_PlanRepositoryImpl : IBNPL_PlanRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public BNPL_PlanRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        //CRUD operations
        //Custom Query Operations
        public async Task<bool> ExistsByBnplPlanTypeAsync(int bnplPlanTypeId)
        {
            return await _context.BNPL_PLANs.AnyAsync(p => p.Bnpl_PlanID == bnplPlanTypeId);
        }
    }
}