using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class TransactionRepositoryImpl : ITransactionRepository
    {
        //CRUD operations
        //Custom Query Operations
        public Task AddAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Transaction>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<PaginationResultDto<Transaction>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? transactionStatusId = null, string? searchKey = null)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Transaction?> UpdateAsync(int id, Transaction transaction)
        {
            throw new NotImplementedException();
        }
    }
}