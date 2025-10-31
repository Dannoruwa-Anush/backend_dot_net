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

        //CRUD operations
        public async Task<IEnumerable<BNPL_PLAN>> GetAllAsync() =>
            await _context.BNPL_PLANs.ToListAsync();

        public async Task<BNPL_PLAN?> GetByIdAsync(int id) =>
            await _context.BNPL_PLANs.FindAsync(id);

        public async Task AddAsync(BNPL_PLAN bNPL_Plan)
        {
            await _context.BNPL_PLANs.AddAsync(bNPL_Plan);
            await _context.SaveChangesAsync();
        }

        public async Task<BNPL_PLAN?> UpdateAsync(int id, BNPL_PLAN updatedPlan)
        {
            var existingPlan = await _context.BNPL_PLANs.FindAsync(id);
            if (existingPlan == null)
                return null;

            existingPlan.Bnpl_Status = updatedPlan.Bnpl_Status;
            existingPlan.UpdatedAt = DateTime.UtcNow;

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
            await _context.SaveChangesAsync();

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
                .Include(p => p.CustomerOrder)
                    .ThenInclude(o => o.Customer)
                .Include(p => p.BNPL_PlanType)
                .AsQueryable();

            // Filter by Plan Status (if provided)
            if (planStatusId.HasValue)
            {
                var statusEnum = (BnplStatusEnum)planStatusId.Value;
                query = query.Where(p => p.Bnpl_Status == statusEnum);
            }

            // Apply search filter (Order ID, PlanType ID, Customer email, phone)
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();

                query = query.Where(p =>
                    p.OrderID.ToString().Contains(searchKey) ||
                    p.Bnpl_PlanTypeID.ToString().Contains(searchKey) ||
                    (p.CustomerOrder.Customer.Email != null && p.CustomerOrder.Customer.Email.ToLower().Contains(searchKey)) ||
                    (p.CustomerOrder.Customer.PhoneNo != null && p.CustomerOrder.Customer.PhoneNo.ToLower().Contains(searchKey))
                );
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
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

         // EF transaction support
        public async Task<IDbContextTransaction> BeginTransactionAsync() =>
            await _context.Database.BeginTransactionAsync();

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}