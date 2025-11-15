using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IBNPL_InstallmentRepository
    {
        //CRUD operations
        Task<IEnumerable<BNPL_Installment>> GetAllAsync();
        Task<BNPL_Installment?> GetByIdAsync(int id);
        Task AddAsync(BNPL_Installment bnpl_installment);
        Task<BNPL_Installment?> UpdateAsync(int id, BNPL_Installment bnpl_installment);

        //Custom Query Operations
        Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null);
        Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationByOrderIdAsync(int orderId, int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null);
        Task<IEnumerable<BNPL_Installment>> GetAllByPlanIdAsync(int planId);
        Task<BNPL_Installment?> GetLatestInstallmentUpToDateAsync(int planId, DateTime asOfDate);
        Task<BNPL_Installment?> GetFirstUpcomingInstallmentAsync(int planId);
        Task<List<BNPL_Installment>> GetAllUnsettledInstallmentUpToDateAsync(int planId, DateTime asOfDate);
        
        //Bulk insert
        Task AddRangeAsync(IEnumerable<BNPL_Installment> installments);
    }
}