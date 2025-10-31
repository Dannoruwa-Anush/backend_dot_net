using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface ITransactionRepository
    {
        //CRUD operations
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<Transaction?> GetByIdAsync(int id);
        Task AddAsync(Transaction transaction);
        Task<Transaction?> UpdateAsync(int id, Transaction transaction);

        //Custom Query Operations
        Task<PaginationResultDto<Transaction>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? transactionStatusId = null, string? searchKey = null);
    }
}