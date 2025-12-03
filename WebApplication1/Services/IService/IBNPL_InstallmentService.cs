using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface IBNPL_InstallmentService
    {
        //CRUD operations
        Task<IEnumerable<BNPL_Installment>> GetAllBNPL_InstallmentsAsync();
        Task<BNPL_Installment?> GetBNPL_InstallmentByIdAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null);
        Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationByOrderIdAsync(int orderId, int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null);
        Task<IEnumerable<BNPL_Installment>> GetAllByPlanIdAsync(int planId);
        Task<List<BNPL_Installment>> GetAllUnsettledInstallmentByPlanIdAsync(int planId);

        //Shared Internal Operations Used by Multiple Repositories
        Task<List<BNPL_Installment>> BuildBnplInstallmentBulkAddRequestAsync(BNPL_PLAN plan);

        //payment
        BnplInstallmentPaymentResultDto BuildBnplInstallmentSettlement(CustomerOrder existingOrder, BnplLatestSnapshotSettledResultDto latestSnapshotSettledResult);
    }
}