using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Utils.Helpers;
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
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<BNPL_Installment>> GetAllAsync() =>
            await _context.BNPL_Installments
                    .Include(bpt => bpt.BNPL_PLAN!)
                        .ThenInclude(ip => ip.BNPL_PlanType)
                    .Include(i => i.BNPL_PLAN!)
                        .ThenInclude(io => io.CustomerOrder)
                    .ToListAsync();

        public async Task<BNPL_Installment?> GetByIdAsync(int id) =>
            await _context.BNPL_Installments
                .Include(i => i.BNPL_PLAN!)
                    .ThenInclude(ip => ip.BNPL_PlanType)
                .Include(i => i.BNPL_PLAN!)
                    .ThenInclude(io => io.CustomerOrder)
                        .ThenInclude(ioc => ioc!.Customer)
                .FirstOrDefaultAsync(i => i.InstallmentID == id);

        public async Task<BNPL_Installment?> UpdateAsync(int id, BNPL_Installment input)
        {
            var existing = await _context.BNPL_Installments.FindAsync(id);
            if (existing == null)
                return null;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // -------------------------
            // Update financial fields
            // -------------------------
            existing.OverPaymentCarried = input.OverPaymentCarried;
            existing.LateInterest = input.LateInterest;
            existing.TotalDueAmount = input.TotalDueAmount;

            bool basePaymentChanged = existing.AmountPaid_AgainstBase != input.AmountPaid_AgainstBase;
            bool arrearsPaymentChanged = existing.AmountPaid_AgainstArrears != input.AmountPaid_AgainstArrears;
            bool latePaymentChanged = existing.AmountPaid_AgainstLateInterest != input.AmountPaid_AgainstLateInterest;

            existing.AmountPaid_AgainstBase = input.AmountPaid_AgainstBase;
            existing.AmountPaid_AgainstArrears = input.AmountPaid_AgainstArrears;
            existing.AmountPaid_AgainstLateInterest = input.AmountPaid_AgainstLateInterest;

            // -------------------------------------
            // Update LastPaymentDate only if needed
            // -------------------------------------
            if (basePaymentChanged || arrearsPaymentChanged || latePaymentChanged)
                existing.LastPaymentDate = now;

            // -------------------------------------
            // Set late interest applied date only 
            // if late interest has changed positively
            //
            // If LateInterest increased OR went above zero, 
            // we update the timestamp.
            // -------------------------------------
            if (input.LateInterest > 0 && input.LateInterest != existing.LateInterest)
                existing.LastLateInterestAppliedDate = now;

            // -------------------------------------
            // Update Installment status
            // -------------------------------------
            bool statusChanged = existing.Bnpl_Installment_Status != input.Bnpl_Installment_Status;
            existing.Bnpl_Installment_Status = input.Bnpl_Installment_Status;
            // Update other related fields depending on status
            if (statusChanged)
            {
                switch (existing.Bnpl_Installment_Status)
                {
                    case BNPL_Installment_StatusEnum.Refunded:
                        existing.RefundDate = now;
                        break;
                }
            }

            _context.BNPL_Installments.Update(existing);
            return existing;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            // Start base query — include related data for searching and display
            var query = _context.BNPL_Installments
                            .Include(i => i.BNPL_PLAN!)
                                .ThenInclude(ip => ip.BNPL_PlanType)
                            .Include(i => i.BNPL_PLAN!)
                                .ThenInclude(io => io.CustomerOrder)
                                    .ThenInclude(ioc => ioc!.Customer)
                            .AsNoTracking()
                            .AsQueryable();

            // Apply filters from helper
            query = ApplyBnpl_Installment_StatusFilter(query, bnpl_Installment_StatusId);
            query = ApplySearch(query, searchKey);

            query = query.OrderByDescending(c => c.CreatedAt);

            // Total count after filters
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

        // Helper method: plan status filter
        private IQueryable<BNPL_Installment> ApplyBnpl_Installment_StatusFilter(IQueryable<BNPL_Installment> query, int? bnpl_Installment_StatusId)
        {
            if (bnpl_Installment_StatusId.HasValue)
            {
                var status = (BNPL_Installment_StatusEnum)bnpl_Installment_StatusId.Value;
                query = query.Where(i => i.Bnpl_Installment_Status == status);
            }

            return query;
        }

        // Helper method: Search filter
        private IQueryable<BNPL_Installment> ApplySearch(IQueryable<BNPL_Installment> query, string? searchKey)
        {
            if (!string.IsNullOrWhiteSpace(searchKey))
            {
                searchKey = searchKey.Trim().ToLower();

                query = query.Where(i =>
                    i.BNPL_PLAN!.OrderID.ToString().Contains(searchKey) ||
                    i.BNPL_PLAN.Bnpl_PlanID.ToString().Contains(searchKey) ||
                    i.BNPL_PLAN.CustomerOrder!.Customer.User.Email.ToLower().Contains(searchKey) ||
                    i.BNPL_PLAN.CustomerOrder.Customer.PhoneNo.ToLower().Contains(searchKey)
                );
            }
            return query;
        }

        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationByOrderIdAsync(int orderId, int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            // Start base query — include related data for searching and display
            var query = _context.BNPL_Installments
                            .Include(i => i.BNPL_PLAN!)
                                .ThenInclude(ip => ip.BNPL_PlanType)
                            .Include(i => i.BNPL_PLAN!)
                                .ThenInclude(io => io.CustomerOrder)
                                    .ThenInclude(ioc => ioc!.Customer)
                            .AsNoTracking()
                            .AsQueryable();

            //filter by order Id
            query = query.Where(i => i.BNPL_PLAN!.CustomerOrder!.OrderID == orderId);

            // Apply filters from helper
            query = ApplyBnpl_Installment_StatusFilter(query, bnpl_Installment_StatusId);
            query = ApplySearch(query, searchKey);

            query = query.OrderByDescending(c => c.CreatedAt);

            // Total count after filters
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
                .Include(i => i.BNPL_PLAN!)
                    .ThenInclude(p => p.CustomerOrder)
                .Where(i => i.Bnpl_PlanID == planId)
                .OrderBy(i => i.InstallmentNo)
                .ToListAsync();
        }

        public async Task<BNPL_Installment?> GetLatestInstallmentUpToDateAsync(int planId, DateTime asOfDate)
        {
            return await _context.BNPL_Installments
                .Where(i => i.Bnpl_PlanID == planId && i.Installment_DueDate <= asOfDate)
                .OrderByDescending(i => i.Installment_DueDate)
                .FirstOrDefaultAsync();
        }

        public async Task<BNPL_Installment?> GetFirstUpcomingInstallmentAsync(int planId)
        {
            return await _context.BNPL_Installments
                .Where(i => i.Bnpl_PlanID == planId)
                .OrderBy(i => i.Installment_DueDate)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BNPL_Installment>> GetAllUnsettledInstallmentUpToDateAsync(int planId, DateTime asOfDate)
        {
            var excludedStatuses = new[]
            {
                BNPL_Installment_StatusEnum.Refunded,
                BNPL_Installment_StatusEnum.Paid_OnTime,
                BNPL_Installment_StatusEnum.Paid_Late
            };

            return await _context.BNPL_Installments
                .Where(i =>
                    i.Bnpl_PlanID == planId &&
                    i.Installment_DueDate <= asOfDate &&
                    !excludedStatuses.Contains(i.Bnpl_Installment_Status)
                )
                .OrderBy(i => i.Installment_DueDate)
                .ToListAsync();
        }

        //Bulk insert
        public async Task AddRangeAsync(List<BNPL_Installment> installments) =>
            await _context.BNPL_Installments.AddRangeAsync(installments);
    }
}