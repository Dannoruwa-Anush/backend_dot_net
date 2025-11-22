using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService
{
    public interface ICashflowService
    {
        //CRUD operations
        Task<IEnumerable<Cashflow>> GetAllCashflowsAsync();
        Task<Cashflow?> GetCashflowByIdAsync(int id);

        //Custom Query Operations
        Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null);
        Task<decimal> SumCashflowsByOrderAsync(int orderId);

        //Shared Internal Operations Used by Multiple Repositories
        Task<Cashflow> BuildCashflowAddRequestAsync(PaymentRequestDto paymentRequest, CashflowTypeEnum cashflowType);
        Task BuildCashflowStatusUpdateRequestAsync(CustomerOrder order, CashflowStatusEnum newStatus, DateTime now);
    }
}