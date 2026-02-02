using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Audit;
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
        private readonly IInvoiceRepository _invoiceRepository;

        // logger: for auditing
        // Audit Logging
        private readonly IAuditLogService _auditLogService;

        // Service-Level (Technical) Logging
        private readonly ILogger<PhysicalShopSessionServiceImpl> _logger;

        // Constructor
        public PhysicalShopSessionServiceImpl(IPhysicalShopSessionRepository repository, IAppUnitOfWork unitOfWork, ICustomerOrderRepository customerOrderRepository, IInvoiceRepository invoiceRepository, IAuditLogService auditLogService, ILogger<PhysicalShopSessionServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _unitOfWork = unitOfWork;
            _customerOrderRepository = customerOrderRepository;
            _invoiceRepository = invoiceRepository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<PhysicalShopSession>> GetAllPhysicalShopSessionsAsync() =>
            await _repository.GetAllAsync();

        public async Task<PhysicalShopSession?> GetPhysicalShopSessionByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<PhysicalShopSession> AddPhysicalShopSessionWithSaveAsync()
        {
            var session = new PhysicalShopSession
            {
                OpenedAt = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow),
                IsActive = true,
                ClosedAt = null
            };

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

        public async Task<PhysicalShopSession> ModifyPhysicalShopSessionWithTransactionAsync(int id)
        {
            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var session = await _repository.GetByIdAsync(id)
                    ?? throw new KeyNotFoundException($"PhysicalShopSession {id} not found.");

                if (!session.IsActive)
                {
                    _logger.LogInformation(
                        "PhysicalShopSession already closed. ID: {Id}", id);
                    return session;
                }

                // ---------------- Close session ----------------
                session.IsActive = false;
                session.ClosedAt = now;

                // ---------------- Cancel unsettled orders ----------------
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

                    // -------- Cancel latest invoice --------
                    var latestInvoice =
                        await _invoiceRepository
                            .GetLatestUnpaidInstallmentInvoiceByOrderIdAsync(order.OrderID);


                    if (latestInvoice != null)
                    {
                        latestInvoice.InvoiceStatus = InvoiceStatusEnum.Voided;
                        latestInvoice.VoidedAt = now;

                        _auditLogService.LogEntityAction(
                            AuditActionTypeEnum.Update,
                            "Invoice",
                            latestInvoice.InvoiceID,
                            "Voided due to physical shop session closure");
                    }

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
                            installment.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Cancelled;
                        }

                        // Cancel ALL settlement summaries
                        foreach (var summary in bnpl.BNPL_PlanSettlementSummaries)
                        {
                            summary.Bnpl_PlanSettlementSummary_Status =
                                BNPL_PlanSettlementSummary_StatusEnum.Cancelled;
                        }
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

        public async Task<PhysicalShopSession?> GetActivePhysicalShopSessionForTodayAsync() =>
            await _repository.GetActiveSessionForTodayAsync();

        public async Task<PhysicalShopSession?> GetLatestActivePhysicalShopSessionAsync() =>
            await _repository.GetLatestActiveSessionAsync();

        public async Task AutoCloseLatestActiveSessionAsync()
        {
            var latestActiveSession =
                await _repository.GetLatestActiveSessionAsync();

            if (latestActiveSession == null)
                throw new InvalidOperationException(
                    "No active physical shop session found to close.");

            await ModifyPhysicalShopSessionWithTransactionAsync(latestActiveSession.PhysicalShopSessionID);
        }
    }
}