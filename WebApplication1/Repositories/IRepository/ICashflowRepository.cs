using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface ICashflowRepository
    {
        //CRUD operations
        Task<IEnumerable<Cashflow>> GetAllAsync();
        Task<Cashflow?> GetByIdAsync(int id);
        Task AddAsync(Cashflow cashflow);

        //Custom Query Operations
        Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null);
        Task<bool> ExistsByCashflowRefAsync(string cashflowRef);
        Task<decimal> SumCashflowsByOrderAsync(int orderId);
    }
}