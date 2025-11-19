using Microsoft.EntityFrameworkCore.Storage;

namespace WebApplication1.Repositories.IRepository
{
    public interface IPaymentRepository
    {
        // EF transaction support
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task SaveChangesAsync();
    }
}