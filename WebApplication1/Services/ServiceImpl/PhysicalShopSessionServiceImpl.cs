using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.UOW.IUOW;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class PhysicalShopSessionServiceImpl : IPhysicalShopSessionService
    {
        private readonly IPhysicalShopSessionRepository _repository;
        private readonly IAppUnitOfWork _unitOfWork;

        private readonly ICustomerOrderRepository _customerOrderRepository;

        // logger: for auditing
        private readonly ILogger<PhysicalShopSessionServiceImpl> _logger;

        // Constructor
        public PhysicalShopSessionServiceImpl(IPhysicalShopSessionRepository repository, IAppUnitOfWork unitOfWork, ICustomerOrderRepository customerOrderRepository, ILogger<PhysicalShopSessionServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerOrderRepository = customerOrderRepository;
            _logger = logger;
        }

        //CRUD operations
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

        public async Task<PhysicalShopSession> ModifyPhysicalShopSessionWithTransactionAsync(int id, PhysicalShopSession session)
        {
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // If closing the session, cancel unsettled orders
                if (!session.IsActive)
                {
                    var unsettledOrders = await _customerOrderRepository.GetAllPaymentPendingByPhysicalShopSessionIdAsync(id);

                    foreach (var order in unsettledOrders)
                    {
                        // ---------------- Order cancellation ----------------
                        order.CancelledDate = now;
                        order.CancellationRequestDate = now;
                        order.CancellationReason = "Physical shop session closed";
                        order.CancellationApproved = true;
                        order.OrderStatus = OrderStatusEnum.Cancelled;
                        order.OrderPaymentStatus = OrderPaymentStatusEnum.Awaiting_Payment;

                        // ---------------- BNPL handling ----------------
                        if (order.OrderPaymentMode == OrderPaymentModeEnum.Pay_Bnpl && order.BNPL_PLAN != null)
                        {
                            var bnpl = order.BNPL_PLAN;

                            bnpl.CancelledAt = now;
                            bnpl.CompletedAt = null;
                            bnpl.Bnpl_Status = BnplStatusEnum.Cancelled;

                            // Cancel ALL installments
                            foreach (var installment in bnpl.BNPL_Installments)
                            {
                                installment.CancelledAt = now;
                                installment.Bnpl_Installment_Status  = BNPL_Installment_StatusEnum.Cancelled;
                            }

                            // Cancel ALL settlement summaries
                            foreach (var summary in bnpl.BNPL_PlanSettlementSummaries)
                            {
                                summary.Bnpl_PlanSettlementSummary_Status =
                                    BNPL_PlanSettlementSummary_StatusEnum.Cancelled;
                            }
                        }
                    }
                }

                // Prevent duplicate active sessions
                if (session.IsActive)
                {
                    var activeSession = await _repository.GetActiveSessionAsync();

                    if (activeSession != null &&
                        activeSession.PhysicalShopSessionID != id)
                    {
                        _logger.LogWarning(
                            "Attempt to activate duplicate PhysicalShopSession. ID: {Id}", id);

                        throw new InvalidOperationException(
                            "Another active physical shop session already exists.");
                    }
                }

                // Update session
                var updated = await _repository.UpdateAsync(id, session);
                if (updated == null)
                    throw new KeyNotFoundException($"PhysicalShopSession with ID {id} not found.");

                // Commit atomic transaction
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("PhysicalShopSession updated successfully. ID: {Id}", id);

                return updated;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}