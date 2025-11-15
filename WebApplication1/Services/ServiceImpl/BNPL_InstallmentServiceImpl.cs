using WebApplication1.DTOs.RequestDto.BnplPaymentSimulation;
using WebApplication1.DTOs.ResponseDto.BnplPaymentSimulation;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_InstallmentServiceImpl : IBNPL_InstallmentService
    {
        private readonly IBNPL_InstallmentRepository _repository;
        private readonly ICustomerOrderRepository _customerOrderRepository;

        //logger: for auditing
        private readonly ILogger<BNPL_InstallmentServiceImpl> _logger;

        // Constructor
        public BNPL_InstallmentServiceImpl(IBNPL_InstallmentRepository repository, ICustomerOrderRepository customerOrderRepository, ILogger<BNPL_InstallmentServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _customerOrderRepository = customerOrderRepository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_Installment>> GetAllBNPL_InstallmentsAsync() =>
            await _repository.GetAllAsync();

        public async Task<BNPL_Installment?> GetBNPL_InstallmentByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);
        }

        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationByOrderIdAsync(int orderId, int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationByOrderIdAsync(orderId, pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);
        }

        //simulator
        public async Task<BnplInstallmentPaymentSimulationResultDto> SimulateBnplInstallmentPaymentAsync(BnplInstallmentPaymentSimulationRequestDto request)
        {
            var customerOrder = await _customerOrderRepository.GetByIdAsync(request.OrderId);
            if (customerOrder == null)
                throw new Exception("Customer order not found.");

            var planId = customerOrder.BNPL_PLAN!.Bnpl_PlanID;

            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // 1. Get the latest installment with due date <= today
            var targetInstallment = await _repository.GetLatestInstallmentUpToDateAsync(planId, today);

            // 2. If none found (all future installments), take the first upcoming
            if (targetInstallment == null)
            {
                targetInstallment = await _repository.GetFirstUpcomingInstallmentAsync(planId);
            }

            if (targetInstallment == null)
                throw new Exception("No installments found for this plan.");

            return await SimulateBnplInstallmentPaymentPerInstallmentAsync(
                targetInstallment.InstallmentID,
                request.PaymentAmount
            );
        }

        //Helper method : simulator per installment
        private async Task<BnplInstallmentPaymentSimulationResultDto> SimulateBnplInstallmentPaymentPerInstallmentAsync(int installmentId, decimal paymentAmount)
        {
            var installment = await _repository.GetByIdAsync(installmentId);
            if (installment == null)
                throw new Exception("Installment not found.");

            if (paymentAmount <= 0)
                throw new Exception("Payment amount must be greater than zero.");

            decimal remaining = paymentAmount;
            decimal paidToArrears = 0m;
            decimal paidToInterest = 0m;
            decimal paidToBase = 0m;

            // ---- Apply Payment Order ----
            // 1. Arrears
            if (installment.ArrearsCarried > 0 && remaining > 0)
            {
                paidToArrears = Math.Min(installment.ArrearsCarried, remaining);
                remaining -= paidToArrears;
            }

            // 2. Late Interest
            if (installment.LateInterest > 0 && remaining > 0)
            {
                paidToInterest = Math.Min(installment.LateInterest, remaining);
                remaining -= paidToInterest;
            }

            // 3. Base Amount
            var baseRemaining = installment.Installment_BaseAmount - installment.AmountPaid;
            if (baseRemaining > 0 && remaining > 0)
            {
                paidToBase = Math.Min(baseRemaining, remaining);
                remaining -= paidToBase;
            }

            // ---- Compute totals ----
            decimal totalApplied = paidToArrears + paidToInterest + paidToBase;
            decimal due = installment.TotalDueAmount;

            decimal remainingBalance;
            decimal overPaymentCarried;
            string resultStatus;

            // Case A: Underpayment (Partial)
            if (totalApplied < due)
            {
                remainingBalance = due - totalApplied;
                overPaymentCarried = 0;
                resultStatus = "Partially Paid";
            }
            // Case B: Exact payment
            else if (totalApplied == due)
            {
                remainingBalance = 0;

                // If remaining > 0, it’s overpayment
                overPaymentCarried = remaining;
                resultStatus = "Fully Settled";
            }
            // Case C: Overpayment
            else
            {
                remainingBalance = 0;

                //leftover payment is the overpayment
                overPaymentCarried = remaining;
                resultStatus = "Fully Settled";
            }

            return new BnplInstallmentPaymentSimulationResultDto
            {
                InstallmentId = installment.InstallmentID,
                InputPayment = paymentAmount,
                PaidToArrears = paidToArrears,
                PaidToInterest = paidToInterest,
                PaidToBase = paidToBase,
                RemainingBalance = remainingBalance,
                OverPaymentCarried = overPaymentCarried,
                ResultStatus = resultStatus
            };
        }

        //Cancel Installment
        public async Task<BNPL_Installment?> CancelInstallmentAsync(int id)
        {
            var installment = await _repository.GetByIdAsync(id)
                ?? throw new Exception("Installment not found.");

            var plan = installment.BNPL_PLAN;
            if (plan?.CustomerOrder == null)
                throw new Exception("Associated order not found.");

            var order = plan.CustomerOrder;

            if (order.ShippedDate != null)
            {
                var deliveredAt = order.DeliveredDate;
                if (deliveredAt != null && DateTime.UtcNow > deliveredAt.Value.AddDays(BnplSystemConstants.FreeTrialPeriodDays))
                    throw new Exception($"Cancellation not allowed after {BnplSystemConstants.FreeTrialPeriodDays} days of delivery.");
            }

            installment.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Cancelled;
            installment.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, installment);
            _logger.LogInformation("Installment {Id} cancelled successfully.", id);

            return installment;
        }

        //calculations
        public async Task<List<BNPL_Installment>> ApplyPaymentToInstallmentAsync(int installmentId, decimal paymentAmount)
        {
            if (paymentAmount <= 0)
                throw new Exception("Payment amount must be greater than zero.");

            var installment = await _repository.GetByIdAsync(installmentId)
                ?? throw new Exception("Installment not found.");

            var planId = installment.Bnpl_PlanID;
            var allInstallments = (await _repository.GetAllByPlanIdAsync(planId))
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            decimal remaining = paymentAmount;
            var updated = new List<BNPL_Installment>();

            _logger.LogInformation("Applying total payment of {Amount} to plan {PlanId}, starting at installment {InstNo}.",
                paymentAmount, planId, installment.InstallmentNo);

            foreach (var inst in allInstallments.Where(i => i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                                                            i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late))
            {
                if (remaining <= 0)
                    break;

                remaining = ApplyPaymentToSingleInstallment(inst, remaining);
                inst.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(inst.InstallmentID, inst);
                updated.Add(inst);

                _logger.LogInformation("Installment {No} processed, remaining balance {Rem}.", inst.InstallmentNo, remaining);
            }

            if (remaining > 0)
            {
                // Still leftover after all installments
                var last = allInstallments.Last();
                last.OverPaymentCarried += remaining;
                await _repository.UpdateAsync(last.InstallmentID, last);
                updated.Add(last);
                _logger.LogInformation("Final overpayment {Rem} carried forward from installment {Id}.", remaining, last.InstallmentID);
            }

            return updated;
        }

        //Helper method
        private decimal ApplyPaymentToSingleInstallment(BNPL_Installment installment, decimal paymentAmount)
        {
            decimal remaining = paymentAmount;
            var now = DateTime.UtcNow;
            bool withinGrace = now <= installment.Installment_DueDate.AddDays(BnplSystemConstants.FreeTrialPeriodDays);

            // Arrears
            if (installment.ArrearsCarried > 0 && remaining > 0)
            {
                decimal pay = Math.Min(installment.ArrearsCarried, remaining);
                installment.ArrearsCarried -= pay;
                remaining -= pay;
            }

            // Late Interest
            if (installment.LateInterest > 0 && remaining > 0)
            {
                decimal pay = Math.Min(installment.LateInterest, remaining);
                installment.LateInterest -= pay;
                remaining -= pay;
            }

            // Base Amount
            var baseRemaining = installment.Installment_BaseAmount - installment.AmountPaid;
            if (baseRemaining > 0 && remaining > 0)
            {
                decimal pay = Math.Min(baseRemaining, remaining);
                installment.AmountPaid += pay;
                remaining -= pay;
            }

            // Determine status
            if (installment.AmountPaid >= installment.TotalDueAmount &&
                installment.ArrearsCarried == 0 &&
                installment.LateInterest == 0)
            {
                installment.Bnpl_Installment_Status = withinGrace
                    ? BNPL_Installment_StatusEnum.Paid_OnTime
                    : BNPL_Installment_StatusEnum.Paid_Late;

                installment.LastPaymentDate = now;
            }
            else if (installment.AmountPaid > 0)
            {
                installment.Bnpl_Installment_Status = withinGrace
                    ? BNPL_Installment_StatusEnum.PartiallyPaid_OnTime
                    : BNPL_Installment_StatusEnum.PartiallyPaid_Late;
            }
            else
            {
                installment.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Pending;
            }

            return remaining;
        }

        public async Task ApplyLateInterestAsync()
        {
            var allInstallments = await _repository.GetAllAsync();
            int updatedCount = 0;

            foreach (var inst in allInstallments)
            {
                // Skip if already paid, cancelled, or refunded
                if (inst.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.Paid_OnTime ||
                    inst.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.Paid_Late ||
                    inst.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.Cancelled ||
                    inst.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.Refunded)
                    continue;

                // Determine overdue (after grace period)
                var overdueDate = inst.Installment_DueDate.AddDays(BnplSystemConstants.FreeTrialPeriodDays);
                if (DateTime.UtcNow <= overdueDate)
                    continue; // not overdue yet

                var planType = inst.BNPL_PLAN?.BNPL_PlanType;
                if (planType == null)
                {
                    _logger.LogWarning("Skipping installment {Id} — missing BNPL plan type reference.", inst.InstallmentID);
                    continue;
                }

                // Calculate late interest
                decimal lateRate = planType.LatePayInterestRate / 100m; // convert from percent to decimal
                decimal basePlusArrears = inst.Installment_BaseAmount + inst.ArrearsCarried;
                decimal interestAmount = basePlusArrears * lateRate;

                // Update fields
                inst.LateInterest = interestAmount;
                inst.TotalDueAmount = inst.Installment_BaseAmount + inst.ArrearsCarried + inst.LateInterest;
                inst.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Overdue;
                inst.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(inst.InstallmentID, inst);
                updatedCount++;

                _logger.LogInformation(
                    "Late interest {Interest:C} applied to installment {InstId}. New total due: {Total:C}",
                    interestAmount, inst.InstallmentID, inst.TotalDueAmount);
            }

            _logger.LogInformation("Late interest update completed. {Count} installments updated.", updatedCount);
        }
    }
}