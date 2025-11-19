using WebApplication1.DTOs.RequestDto.Payment.Bnpl;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
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

        //Payment : Main Driver
        public async Task<BnplInstallmentPaymentResultDto> ApplyBnplPaymentAsync(BNPL_InstallmentPaymentRequestDto request)
        {
            // Load the customer order
            var order = await _customerOrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new Exception("Customer order not found.");

            var planId = order.BNPL_PLAN!.Bnpl_PlanID;

            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Get unsettled installments up to today
            var unsettledInstallments = await _repository.GetAllUnsettledInstallmentUpToDateAsync(planId, today);

            if (!unsettledInstallments.Any())
                throw new Exception("No unsettled installments found.");

            var response = new BnplInstallmentPaymentResultDto();
            decimal remaining = request.PaymentAmount;

            foreach (var inst in unsettledInstallments)
            {
                if (remaining <= 0)
                    break;

                var breakdown = ApplyPaymentToSingleInstallment(inst, ref remaining);
                response.PerInstallmentBreakdown.Add(breakdown);
            }

            //await _repository.SaveChangesAsync();
            return response;
        }

        //Helper Method : 
        private BnplPerInstallmentPaymentBreakdownResultDto ApplyPaymentToSingleInstallment(BNPL_Installment inst, ref decimal remainingPayment)
        {
            var breakdown = new BnplPerInstallmentPaymentBreakdownResultDto
            {
                InstallmentId = inst.InstallmentID
            };

            // ----------- STEP 1: Arrears -----------
            decimal arrears = inst.ArrearsCarried;

            if (arrears > 0 && remainingPayment > 0)
            {
                var applied = Math.Min(arrears, remainingPayment);
                inst.AmountPaid += applied;
                remainingPayment -= applied;
                breakdown.AppliedToArrears = applied;
            }

            // Refresh values
            decimal lateInterest = inst.LateInterest;
            decimal baseRemaining = inst.Installment_BaseAmount - inst.AmountPaid;

            // ----------- STEP 2: Late Interest -----------
            if (lateInterest > 0 && remainingPayment > 0)
            {
                var applied = Math.Min(lateInterest, remainingPayment);
                inst.LateInterest -= applied;
                remainingPayment -= applied;
                breakdown.AppliedToLateInterest = applied;
            }

            // Refresh base after interest
            baseRemaining = inst.Installment_BaseAmount - inst.AmountPaid;

            // ----------- STEP 3: Base Amount -----------
            if (baseRemaining > 0 && remainingPayment > 0)
            {
                var applied = Math.Min(baseRemaining, remainingPayment);
                inst.AmountPaid += applied;
                remainingPayment -= applied;
                breakdown.AppliedToBase = applied;
            }

            // ----------- STEP 4: Overpayment -----------
            if (remainingPayment > 0)
            {
                inst.OverPaymentCarried += remainingPayment;
                breakdown.OverPayment = remainingPayment;
                remainingPayment = 0;
            }

            // ----------- Status Update -----------
            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
            bool fullyPaid = inst.AmountPaid >= inst.Installment_BaseAmount && inst.LateInterest <= 0;
            bool overdue = inst.Installment_DueDate < today;

            if (fullyPaid)
            {
                inst.Bnpl_Installment_Status =
                    overdue ? BNPL_Installment_StatusEnum.Paid_Late : BNPL_Installment_StatusEnum.Paid_OnTime;
            }
            else
            {
                if (inst.AmountPaid > 0)
                {
                    inst.Bnpl_Installment_Status =
                        overdue ? BNPL_Installment_StatusEnum.PartiallyPaid_Late : BNPL_Installment_StatusEnum.PartiallyPaid_OnTime;
                }
                else
                {
                    inst.Bnpl_Installment_Status =
                        overdue ? BNPL_Installment_StatusEnum.Overdue : BNPL_Installment_StatusEnum.Pending;
                }
            }

            breakdown.NewStatus = inst.Bnpl_Installment_Status.ToString();
            return breakdown;
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

        //Handle : Overdue Installments
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
                decimal lateRate = planType.LatePayInterestRatePerDay / 100m; // convert from percent to decimal
                decimal basePlusArrears = inst.Installment_BaseAmount /*+ inst.ArrearsCarried*/;
                decimal interestAmount = basePlusArrears * lateRate;

                // Update fields
                inst.LateInterest = interestAmount;
                inst.TotalDueAmount = inst.Installment_BaseAmount + /*inst.ArrearsCarried*/ +inst.LateInterest;
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