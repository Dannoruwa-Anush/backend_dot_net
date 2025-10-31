using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;

namespace WebApplication1.Repositories.IRepository
{
    public interface IPaymentRepository
    {
        //CRUD operations
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(int id);
        Task AddAsync(Payment payment);
        Task<Payment?> UpdateAsync(int id, Payment payment);

        //Custom Query Operations
        Task<PaginationResultDto<Payment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? transactionStatusId = null, string? searchKey = null);
    }
}