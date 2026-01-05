using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;

namespace WebApplication1.Repositories.RepositoryImpl
{
    public class PhysicalShopSessionRepositoryImpl : IPhysicalShopSessionRepository
    {
        private readonly AppDbContext _context;

        // Constructor
        public PhysicalShopSessionRepositoryImpl(AppDbContext context)
        {
            // Dependency injection
            _context = context;
        }
        // Note : SaveChangesAsync() of Add, Update, Delete will be handled by UOW

        //CRUD operations
        public async Task<IEnumerable<PhysicalShopSession>> GetAllAsync() =>
            await _context.PhysicalShopSessions.ToListAsync();

        public async Task<PhysicalShopSession?> GetByIdAsync(int id) =>
            await _context.PhysicalShopSessions.FindAsync(id);

        public async Task AddAsync(PhysicalShopSession physicalShopSession) =>
            await _context.PhysicalShopSessions.AddAsync(physicalShopSession);

        public async Task<PhysicalShopSession?> UpdateAsync(int id, PhysicalShopSession physicalShopSession)
        {
            var existing = await _context.PhysicalShopSessions.FindAsync(id);
            if (existing == null)
                return null;

            existing.OpenedAt = physicalShopSession.OpenedAt;
            existing.IsActive = physicalShopSession.IsActive;
            existing.ClosedAt = physicalShopSession.ClosedAt;
            _context.PhysicalShopSessions.Update(existing);
            return existing;
        }

        public async Task<PhysicalShopSession?> GetActiveSessionAsync()
        {
            return await _context.PhysicalShopSessions
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.IsActive);
        }

        public async Task<bool> HasActiveSessionAsync()
        {
            return await _context.PhysicalShopSessions
                .AnyAsync(s => s.IsActive && s.ClosedAt == null);
        }
    }
}