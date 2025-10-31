using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class BNPL_InstallmentRepositoryImpl : IBNPL_InstallmentRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public BNPL_InstallmentRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_Installment>> GetAllAsync() =>
            await _context.BNPL_Installments.ToListAsync();

        public async Task<BNPL_Installment?> GetByIdAsync(int id) =>
            await _context.BNPL_Installments.FindAsync(id);

        public async Task AddAsync(BNPL_Installment bnpl_installment)
        {
            await _context.BNPL_Installments.AddAsync(bnpl_installment);
            await _context.SaveChangesAsync();
        }

        public async Task<BNPL_Installment?> UpdateAsync(int id, BNPL_Installment bnpl_installment)
        {
            var existing = await _context.BNPL_Installments.FindAsync(id);
            if (existing == null)
                return null;

            existing.Bnpl_Installment_Status = bnpl_installment.Bnpl_Installment_Status;

            _context.BNPL_Installments.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            // Start base query — include related data for searching and display
            var query = _context.BNPL_Installments
                .Include(i => i.BNPL_PLAN)
                    .ThenInclude(p => p.CustomerOrder)
                        .ThenInclude(o => o.Customer)
                .AsQueryable();

            // Filter: by Installment Status
            if (bnpl_Installment_StatusId.HasValue)
            {
                var status = (BNPL_Installment_StatusEnum)bnpl_Installment_StatusId.Value;
                query = query.Where(i => i.Bnpl_Installment_Status == status);
            }

            // Filter: by Search Key (OrderID, PlanID, Email, Phone)
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();

                query = query.Where(i =>
                    i.BNPL_PLAN.OrderID.ToString().Contains(searchKey) ||
                    i.BNPL_PLAN.Bnpl_PlanID.ToString().Contains(searchKey) ||
                    i.BNPL_PLAN.CustomerOrder.Customer.Email.ToLower().Contains(searchKey) ||
                    i.BNPL_PLAN.CustomerOrder.Customer.PhoneNo.ToLower().Contains(searchKey)
                );
            }

            // Get total count (after filters)
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Return paginated result
            return new PaginationResultDto<BNPL_Installment>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<BNPL_Installment>> GetAllByPlanIdAsync(int planId)
        {
            return await _context.BNPL_Installments
                .Include(i => i.BNPL_PLAN)
                    .ThenInclude(p => p.CustomerOrder)
                .Where(i => i.Bnpl_PlanID == planId)
                .OrderBy(i => i.InstallmentNo)
                .ToListAsync();
        }

        //Bulk insert
        public async Task AddRangeAsync(IEnumerable<BNPL_Installment> installments)
        {
            await _context.BNPL_Installments.AddRangeAsync(installments);
        }
    }
}