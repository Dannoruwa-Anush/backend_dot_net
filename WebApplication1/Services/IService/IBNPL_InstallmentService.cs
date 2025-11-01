using WebApplication1.DTOs.RequestDto.BnplPaymentSimulation;
using WebApplication1.DTOs.ResponseDto.BnplPaymentSimulation;
using WebApplication1.DTOs.ResponseDto.Common;
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
        Task<BNPL_Installment?> CancelInstallmentAsync(int id);

        //calculations
        Task<List<BNPL_Installment>> ApplyPaymentToInstallmentAsync(int installmentId, decimal paymentAmount);
        Task ApplyLateInterestAsync();

        //simulator
        Task<BnplInstallmentPaymentSimulationResultDto> SimulateBnplInstallmentPaymentAsync(BnplInstallmentPaymentSimulationRequestDto request);
    }
}