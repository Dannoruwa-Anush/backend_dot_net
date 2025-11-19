using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Services.IService
{
    public interface ICashflowService
    {
        //CRUD operations
        Task<IEnumerable<Cashflow>> GetAllCashflowsAsync();
        Task<Cashflow?> GetCashflowByIdAsync(int id);
        //Task<Cashflow> AddCashflowAsync(CashflowRequestDto request);
        Task<Cashflow?> UpdateCashflowAsync(int id, Cashflow cashflow);

        //Custom Query Operations
        Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null);
    }
}