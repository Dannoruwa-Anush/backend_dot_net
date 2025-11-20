namespace WebApplication1.UOW.IUOW
{
    public interface IAppUnitOfWork
    {
        // Unit of Work (UoW) is a design pattern used to manage and coordinate 
        // changes across multiple repositories in a single transaction
        // Repository - Unit of Work - Service - Controller.

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();

        // Saves changes without requiring a transaction.
        Task<int> SaveChangesAsync();
    }
}