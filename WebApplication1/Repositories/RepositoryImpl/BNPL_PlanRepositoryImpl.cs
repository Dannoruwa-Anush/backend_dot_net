using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Project_Enums;

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
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<BNPL_PLAN>> GetAllAsync() =>
            await _context.BNPL_PLANs
                    .Include(bpl => bpl.BNPL_PlanType)
                    .ToListAsync();

        public async Task<BNPL_PLAN?> GetByIdAsync(int id) =>
            await _context.BNPL_PLANs
                    .Include(bpl => bpl.BNPL_PlanType)
                    .Include(bpl => bpl.CustomerOrder)
                        .ThenInclude(bplC => bplC!.Customer)
                    .FirstOrDefaultAsync(bpl => bpl.Bnpl_PlanID == id);

        public async Task AddAsync(BNPL_PLAN bNPL_Plan) =>
            await _context.BNPL_PLANs.AddAsync(bNPL_Plan);

        public async Task<BNPL_PLAN?> UpdateAsync(int id, BNPL_PLAN updatedPlan)
        {
            var existingPlan = await _context.BNPL_PLANs.FindAsync(id);
            if (existingPlan == null)
                return null;

            existingPlan.Bnpl_Status = updatedPlan.Bnpl_Status;

            // Update other related fields depending on status
            switch (updatedPlan.Bnpl_Status)
            {
                case BnplStatusEnum.Completed:
                    existingPlan.CompletedAt = DateTime.UtcNow;
                    break;

                case BnplStatusEnum.Cancelled:
                case BnplStatusEnum.Refunded:
                    existingPlan.CancelledAt = DateTime.UtcNow;
                    break;
            }

            _context.BNPL_PLANs.Update(existingPlan);
            return existingPlan;
        }

        //Custom Query Operations
        public async Task<bool> ExistsByBnplPlanTypeAsync(int bnplPlanTypeId)
        {
            return await _context.BNPL_PLANs.AnyAsync(p => p.Bnpl_PlanID == bnplPlanTypeId);
        }

        public async Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null)
        {
            var query = _context.BNPL_PLANs
                        .Include(bpl => bpl.BNPL_PlanType)
                        .AsNoTracking()
                        .AsQueryable();

            // Apply filters from helper
            query = ApplyPlanStatusFilter(query, planStatusId);
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
            return new PaginationResultDto<BNPL_PLAN>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Helper method: plan status filter
        private IQueryable<BNPL_PLAN> ApplyPlanStatusFilter(IQueryable<BNPL_PLAN> query, int? planStatusId)
        {
            if (planStatusId.HasValue)
            {
                var statusEnum = (BnplStatusEnum)planStatusId.Value;
                query = query.Where(p => p.Bnpl_Status == statusEnum);
            }

            return query;
        }

        // Helper method: Search filter (email, phone, order date)
        private IQueryable<BNPL_PLAN> ApplySearch(IQueryable<BNPL_PLAN> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();

                query = query.Where(p =>
                    p.OrderID.ToString().Contains(searchKey) ||
                    p.Bnpl_PlanTypeID.ToString().Contains(searchKey) ||
                    (p.CustomerOrder!.Customer.User.Email != null && p.CustomerOrder.Customer.User.Email.ToLower().Contains(searchKey)) ||
                    (p.CustomerOrder.Customer.PhoneNo != null && p.CustomerOrder.Customer.PhoneNo.ToLower().Contains(searchKey))
                );
            }
            return query;
        }

        public async Task<BNPL_PLAN?> GetByOrderIdAsync(int orderId) =>
            await _context.BNPL_PLANs
                .FirstOrDefaultAsync(b => b.OrderID == orderId);
    }
}