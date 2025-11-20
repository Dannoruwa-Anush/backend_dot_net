using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Data;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.UOW.UOWImpl
{
    public class AppUnitOfWorkImpl : IAppUnitOfWork
    {
        // Unit of Work (UoW) is a design pattern used to manage and coordinate 
        // changes across multiple repositories in a single transaction
        // Repository - Unit of Work - Service - Controller.

        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // Constructor
        public AppUnitOfWorkImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            // Save pending DB changes
            await _context.SaveChangesAsync();

            // If in transaction, commit safely
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        // Saves changes without requiring a transaction.
        public Task<int> SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}