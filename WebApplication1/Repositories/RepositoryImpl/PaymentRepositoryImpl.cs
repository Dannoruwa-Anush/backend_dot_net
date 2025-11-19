using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Data;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class PaymentRepositoryImpl : IPaymentRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public PaymentRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        // EF transaction support
        public async Task<IDbContextTransaction> BeginTransactionAsync() =>
            await _context.Database.BeginTransactionAsync();

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}