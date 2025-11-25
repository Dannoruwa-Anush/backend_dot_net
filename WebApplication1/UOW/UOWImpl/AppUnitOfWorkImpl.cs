using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WebApplication1.Data;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.UOW.UOWImpl
{
    public class AppUnitOfWorkImpl : IAppUnitOfWork, IAsyncDisposable
    {
        /* 
            Unit of Work (UoW) is a design pattern used to manage and coordinate 
            changes across multiple repositories in a single transaction
            Repository - Unit of Work - Service - Controller.
        */

        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        // Constructor
        public AppUnitOfWorkImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }

        /*
            Save single table changes to the database immediately without requiring a transaction.
            SaveChangesAsync() : EF Core method
            When to use : Single Repository operations
        */
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await HandleConcurrency(ex);
                throw new DBConcurrencyException("Data was changed by another user.");
            }
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            if (_transaction != null)
                return _transaction; // already started

            _transaction = await _context.Database.BeginTransactionAsync();
            return _transaction;
        }

        /*
            Save multipe table changes to the database as a single atomic opeartion with requiring a transaction.
            CommitAsync() : Unit of Work method
            When to use : Multiple Repository operations
        */
        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync() first.");

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await HandleConcurrency(ex);
                throw new DBConcurrencyException("Data was changed by another user.");
            }
            finally
            {
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

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();
        }

        //Helper : to handle concurrency
        private async Task HandleConcurrency(DbUpdateConcurrencyException ex)
        {
            foreach (var entry in ex.Entries)
            {
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    // Row has been deleted
                    throw new Exception("Record was deleted by another user.");
                }

                // Update the OriginalValues to reflect DB state
                entry.OriginalValues.SetValues(databaseValues);
            }
        }
    }
}
