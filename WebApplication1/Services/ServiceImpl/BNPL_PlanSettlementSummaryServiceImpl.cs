using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_PlanSettlementSummaryServiceImpl : IBNPL_PlanSettlementSummaryService
    {
        private readonly IBNPL_PlanSettlementSummaryRepository _repository;

        private readonly ICustomerOrderRepository _customerOrderRepository;

        // logger: for auditing
        private readonly ILogger<BNPL_PlanSettlementSummaryServiceImpl> _logger;

        // Constructor
        public BNPL_PlanSettlementSummaryServiceImpl(IBNPL_PlanSettlementSummaryRepository repository, ICustomerOrderRepository customerOrderRepository, ILogger<BNPL_PlanSettlementSummaryServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _customerOrderRepository = customerOrderRepository;
            _logger = logger;
        }

        //Custom Query Operations
        public Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId) =>
            _repository.GetLatestSnapshotWithOrderDetailsAsync(orderId);

        //simulator : Main Driver
        public async Task<BnplSnapshotPayingSimulationResultDto> SimulateBnplPlanSettlementAsync(BnplSnapshotPayingSimulationRequestDto request)
        {
            var latestSnapshot = await _repository.GetLatestSnapshotWithOrderDetailsAsync(request.OrderId);
            if (latestSnapshot == null)
                throw new Exception("Latest snapshot not found");

            return SimulatePaymentAllocationForSnapshot(latestSnapshot, request.PaymentAmount);
        }

        //Helper method : simulator per installment
        private BnplSnapshotPayingSimulationResultDto SimulatePaymentAllocationForSnapshot(BNPL_PlanSettlementSummary snapshot, decimal paymentAmount)
        {
            //Call helper method
            (decimal paidToArrears, decimal paidToInterest, decimal paidToBase, decimal remainingBalance, decimal overPayment) = AllocatePaymentBuckets(snapshot, paymentAmount);

            string status =
                remainingBalance == 0 && overPayment > 0 ? "Overpaid" :
                remainingBalance == 0 ? "Fully Settled" : "Partially Paid";

            return new BnplSnapshotPayingSimulationResultDto
            {
                InstallmentId = snapshot.CurrentInstallmentNo,
                PaidToArrears = paidToArrears,
                PaidToInterest = paidToInterest,
                PaidToBase = paidToBase,
                RemainingBalance = remainingBalance,
                OverPaymentCarried = overPayment,
                ResultStatus = status
            };
        }

        //Helper : AllocatePaymentBuckets
        private (decimal paidArrears, decimal paidInterest, decimal paidBase, decimal remainingBalance, decimal overPayment) AllocatePaymentBuckets(BNPL_PlanSettlementSummary snapshot, decimal paymentAmount)
        {
            if (paymentAmount <= 0)
                throw new Exception("Payment amount should be a positive number");

            decimal remainingArrears = Math.Max(0, snapshot.Total_InstallmentBaseArrears - snapshot.Paid_AgainstTotalArrears);
            decimal remainingInterest = Math.Max(0, snapshot.Total_LateInterest - snapshot.Paid_AgainstTotalLateInterest);
            decimal remainingBase = Math.Max(0, snapshot.NotYetDueCurrentInstallmentBaseAmount - snapshot.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount);

            //Call helper method
            var (paidArrears, paidInterest, paidBase, leftover) = ApplyPayingFlow(paymentAmount, remainingArrears, remainingInterest, remainingBase);

            //Calculate : remainingBalance
            decimal remainingBalance = Math.Max(0, remainingArrears + remainingInterest + remainingBase - (paidArrears + paidInterest + paidBase));

            return (paidArrears, paidInterest, paidBase, remainingBalance, leftover);
        }

        //Helper : paying flow
        private (decimal paidArrears, decimal paidInterest, decimal paidBase, decimal remaining) ApplyPayingFlow(decimal amount, decimal arrears, decimal interest, decimal baseAmount)
        {
            decimal remaining = amount;
            decimal paidArrears = 0m;
            decimal paidInterest = 0m;
            decimal paidBase = 0m;

            // 1. Arrears first
            if (remaining > 0 && arrears > 0)
            {
                paidArrears = Math.Min(arrears, remaining);
                remaining -= paidArrears;
            }

            // 2. Late Interest
            if (remaining > 0 && interest > 0)
            {
                paidInterest = Math.Min(interest, remaining);
                remaining -= paidInterest;
            }

            // 3. Current / Not-yet-due Base
            if (remaining > 0 && baseAmount > 0)
            {
                paidBase = Math.Min(baseAmount, remaining);
                remaining -= paidBase;
            }

            return (paidArrears, paidInterest, paidBase, remaining);
        }

        //Shared Internal Operations Used by Multiple Repositories
        //-----------------[Start: snapshot payment]--------------------
        public BnplLatestSnapshotSettledResultDto BuildBNPL_PlanLatestSettlementSummaryUpdateRequestAsync(CustomerOrder existingOrder, decimal paymentAmount)
        {
            var existingPlan = existingOrder.BNPL_PLAN
                ?? throw new Exception("BNPL plan not found on order");

            var latestSnapshot = GetLatestSnapshot(existingPlan);

            var allocation = AllocatePaymentBuckets(latestSnapshot, paymentAmount);

            UpdateSnapshotWithAllocation(latestSnapshot, allocation);

            var snapshotResult = BuildSettlementResultDto(allocation);

            return snapshotResult;
        }

        //Helper method
        private BNPL_PlanSettlementSummary GetLatestSnapshot(BNPL_PLAN plan)
        {
            return plan.BNPL_PlanSettlementSummaries.FirstOrDefault(s => s.IsLatest)
                   ?? throw new Exception("Latest snapshot not found");
        }

        //Helper method
        private void UpdateSnapshotWithAllocation(BNPL_PlanSettlementSummary snapshot, (decimal paidArrears, decimal paidInterest, decimal paidBase, decimal remainingBalance, decimal overPayment) allocation)
        {
            snapshot.Paid_AgainstTotalArrears += allocation.paidArrears;
            snapshot.Paid_AgainstTotalLateInterest += allocation.paidInterest;
            snapshot.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount += allocation.paidBase;

            var totalAllocated = allocation.paidArrears + allocation.paidInterest + allocation.paidBase;
            snapshot.Total_PayableSettlement = Math.Max(snapshot.Total_PayableSettlement - totalAllocated, 0m);
            snapshot.Total_OverpaymentCarriedToNext += allocation.overPayment;

            if (allocation.remainingBalance == 0)
            {
                snapshot.Bnpl_PlanSettlementSummary_PaymentStatus = Bnpl_PlanSettlementSummary_PaymentStatusEnum.Settled;
            }
        }

        //Helper method
        private BnplLatestSnapshotSettledResultDto BuildSettlementResultDto((decimal paidArrears, decimal paidInterest, decimal paidBase, decimal remainingBalance, decimal overPayment) allocation)
        {
            return new BnplLatestSnapshotSettledResultDto
            {
                TotalPaidArrears = allocation.paidArrears,
                TotalPaidLateInterest = allocation.paidInterest,
                TotalPaidCurrentInstallmentBase = allocation.paidBase,
                OverPaymentCarriedToNextInstallment = allocation.overPayment
            };
        }
        //-----------------[End: snapshot payment]----------------------


        ///*********************************************** need to check again*************************
        public BNPL_PlanSettlementSummary? BuildSettlementGenerateRequestForPlanAsync(BNPL_PLAN existingPlan)
        {
            var installments = existingPlan.BNPL_Installments;

            if (installments == null || installments.Count == 0)
                return null;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            //Filter unpaid installments
            var unpaid = installments
                .Where(i => i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                            i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late &&
                            i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Refunded)
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            if (!unpaid.Any())
                return null;

            // Next upcoming installment (future due date)
            var nextUpcoming = unpaid.FirstOrDefault(i => i.Installment_DueDate >= now);

            // Only process installments up to the upcoming one
            int maxInstallmentNoAllowed =
                nextUpcoming?.InstallmentNo ?? unpaid.Last().InstallmentNo;

            var effectiveInstallments = unpaid
                .Where(i => i.InstallmentNo <= maxInstallmentNoAllowed)
                .ToList();

            //Accumulators
            decimal arrearsBase = 0m;
            decimal notYetDueBase = 0m;
            decimal totalLateInterest = 0m;

            // Payment reporting only (not used in settlement computation)
            decimal paidAgainstArrears = 0m;
            decimal paidAgainstNotYetDue = 0m;
            decimal paidAgainstLateInterest = 0m;

            foreach (var inst in effectiveInstallments)
            {
                bool isArrears = inst.Installment_DueDate < now;

                decimal remainingBase =
                    Math.Max(0, inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase);

                decimal remainingInterest =
                    Math.Max(0, inst.LateInterest - inst.AmountPaid_AgainstLateInterest);

                if (isArrears)
                    paidAgainstArrears += inst.AmountPaid_AgainstBase;
                else
                    paidAgainstNotYetDue += inst.AmountPaid_AgainstBase;

                paidAgainstLateInterest += inst.AmountPaid_AgainstLateInterest;

                if (isArrears)
                    arrearsBase += remainingBase;
                else
                    notYetDueBase += remainingBase;

                totalLateInterest += remainingInterest;
            }

            decimal totalPayable = arrearsBase + notYetDueBase + totalLateInterest;

            MarkLatestSnapshotObsolete(existingPlan);

            return new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = effectiveInstallments.First().Bnpl_PlanID,
                CurrentInstallmentNo = effectiveInstallments.First().InstallmentNo,

                Total_InstallmentBaseArrears = arrearsBase,
                NotYetDueCurrentInstallmentBaseAmount = notYetDueBase,
                Total_LateInterest = totalLateInterest,

                // Reporting-only fields
                Paid_AgainstTotalArrears = paidAgainstArrears,
                Paid_AgainstNotYetDueCurrentInstallmentBaseAmount = paidAgainstNotYetDue,
                Paid_AgainstTotalLateInterest = paidAgainstLateInterest,

                Total_PayableSettlement = totalPayable,

                IsLatest = true,
                Bnpl_PlanSettlementSummaryRef = $"SNP-{effectiveInstallments.First().Bnpl_PlanID}-for-{effectiveInstallments.First().InstallmentNo}-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}"
            };
        }

        //Helper to mark last snapshot as Obsolete
        private void MarkLatestSnapshotObsolete(BNPL_PLAN bnplPlan)
        {
            if (bnplPlan == null)
                throw new ArgumentNullException(nameof(bnplPlan));

            var latestSnapshot = bnplPlan.BNPL_PlanSettlementSummaries
                .FirstOrDefault(s => s.IsLatest);

            // If no snapshot exists yet (first time) - nothing to obsolete
            if (latestSnapshot == null)
                return;

            latestSnapshot.IsLatest = false;
            latestSnapshot.Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Obsolete;
        }
    }
}