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
        Task AddCashflowAsync(CustomerOrder order, decimal amountPaid, CashflowTypeEnum type, int? installmentNo = null, bool isFullInstallmentPayment = false);
        Task<Cashflow?> UpdateCashflowAsync(int id, Cashflow cashflow);

        //Custom Query Operations
        Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null);
    }
}