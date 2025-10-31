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
        Task<BNPL_Installment?> CancelInstallmentAsync(int id);
        Task<List<BNPL_Installment>> ApplyPaymentToInstallmentAsync(int installmentId, decimal paymentAmount);
    }
}