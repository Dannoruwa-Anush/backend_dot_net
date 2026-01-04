using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;

namespace WebApplication1.Services.ServiceImpl
{
    public class PhysicalShopSessionServiceImpl : IPhysicalShopSessionService
    {
        private readonly IPhysicalShopSessionRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        // logger: for auditing
        private readonly ILogger<PhysicalShopSessionServiceImpl> _logger;

        // Constructor
        public PhysicalShopSessionServiceImpl(IPhysicalShopSessionRepository repository, IAppUnitOfWork unitOfWork, ILogger<PhysicalShopSessionServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<PhysicalShopSession>> GetAllPhysicalShopSessionsAsync() =>
            await _repository.GetAllAsync();

        public async Task<PhysicalShopSession?> GetPhysicalShopSessionByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<PhysicalShopSession> AddPhysicalShopSessionWithSaveAsync(PhysicalShopSession session)
        {
            // Business rule: Only one active session allowed
            if (session.IsActive)
            {
                var hasActiveSession = await _repository.HasActiveSessionAsync();
                if (hasActiveSession)
                {
                    _logger.LogWarning("Attempt to create duplicate active PhysicalShopSession.");
                    throw new InvalidOperationException("An active physical shop session already exists.");
                }
            }

            await _repository.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("PhysicalShopSession created. ID: {Id}", session.PhysicalShopSessionID);
            return session;
        }

        public async Task<PhysicalShopSession> UpdatePhysicalShopSessionWithSaveAsync(int id, PhysicalShopSession session)
        {
            if (session.IsActive)
            {
                var activeSession = await _repository.GetActiveSessionAsync();

                if (activeSession != null && activeSession.PhysicalShopSessionID != id)
                {
                    _logger.LogWarning("Attempt to activate duplicate PhysicalShopSession. ID: {Id}", id);
                    throw new InvalidOperationException("Another active physical shop session already exists.");
                }
            }

            var updated = await _repository.UpdateAsync(id, session);
            if (updated == null)
                throw new KeyNotFoundException($"PhysicalShopSession with ID {id} not found.");

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("PhysicalShopSession updated. ID: {Id}", id);
            return updated;
        }
    }
}