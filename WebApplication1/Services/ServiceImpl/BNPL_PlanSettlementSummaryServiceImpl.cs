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

        //Helper : paying flow
        private (decimal paidArrears, decimal paidInterest, decimal paidBase, decimal remainingBalance, decimal overPayment) AllocatePaymentBuckets(BNPL_PlanSettlementSummary snapshot, decimal paymentAmount)
        {
            decimal remaining = paymentAmount;

            decimal remainingArrears = Math.Max(0, snapshot.Total_InstallmentBaseArrears - snapshot.Paid_AgainstTotalArrears);

            decimal remainingInterest = Math.Max(0, snapshot.Total_LateInterest - snapshot.Paid_AgainstTotalLateInterest);

            decimal remainingBase = Math.Max(0, snapshot.NotYetDueCurrentInstallmentBaseAmount - snapshot.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount);

            decimal totalRemainingDue = remainingArrears + remainingInterest + remainingBase;

            // -----------------------------
            // TRACK CURRENT PAYMENT ALLOCATION
            // -----------------------------
            decimal paidArrears = 0m;
            decimal paidInterest = 0m;
            decimal paidBase = 0m;

            // -----------------------------
            // 1. PAY ARREARS
            // -----------------------------
            if (remaining > 0 && remainingArrears > 0)
            {
                paidArrears = Math.Min(remainingArrears, remaining);
                remaining -= paidArrears;
            }

            // -----------------------------
            // 2. PAY LATE INTEREST
            // -----------------------------
            if (remaining > 0 && remainingInterest > 0)
            {
                paidInterest = Math.Min(remainingInterest, remaining);
                remaining -= paidInterest;
            }

            // -----------------------------
            // 3. PAY CURRENT INSTALLMENT BASE
            // -----------------------------
            if (remaining > 0 && remainingBase > 0)
            {
                paidBase = Math.Min(remainingBase, remaining);
                remaining -= paidBase;
            }

            // -----------------------------
            // SUMMARY
            // -----------------------------
            decimal appliedTotal = paidArrears + paidInterest + paidBase;

            decimal remainingBalance = Math.Max(0, totalRemainingDue - appliedTotal);
            decimal overPayment = Math.Max(0, remaining);

            return (paidArrears, paidInterest, paidBase, remainingBalance, overPayment);
        }

        //Shared Internal Operations Used by Multiple Repositories
        public (BnplLastSnapshotSettledResultDto, BNPL_PlanSettlementSummary) BuildBNPL_PlanLastSettlementSummaryUpdateRequestAsync(BNPL_PlanSettlementSummary lastSnapshot, decimal paymentAmount)
        {
            //Call helper method
            (decimal paidArrears, decimal paidInterest, decimal paidBase, decimal remainingBalance, decimal nextSnapshotOverPayment) = AllocatePaymentBuckets(lastSnapshot, paymentAmount);

            // Update only what belongs to this snapshot
            lastSnapshot.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount += paidBase;
            lastSnapshot.Paid_AgainstTotalArrears += paidArrears;
            lastSnapshot.Paid_AgainstTotalLateInterest += paidInterest;

            // DO NOT ACCUMULATE -- assign once
            lastSnapshot.Total_OverpaymentCarriedToNext = nextSnapshotOverPayment;

            lastSnapshot.Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Obsolete;
            lastSnapshot.IsLatest = false;

            var totalSettlement = new BnplLastSnapshotSettledResultDto
            {
                TotalPaidArrears = paidArrears,
                TotalPaidLateInterest = paidInterest,
                TotalPaidCurrentInstallmentBase = paidBase,
                OverPaymentCarriedToNextInstallment = nextSnapshotOverPayment
            };

            return (totalSettlement, lastSnapshot);
        }













        ///*********************************************** need to check again*************************
        public BNPL_PlanSettlementSummary? BuildSettlementGenerateRequestForPlanAsync(BNPL_PLAN existingPlan)
        {
            var installments = existingPlan.BNPL_Installments;

            if (installments == null || installments.Count == 0)
                return null;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            MarkPreviousSnapshotsOfPlanAsObsoleteAsync(existingPlan);

            // Step 1: filter paid/refunded
            var unpaid = installments
                .Where(inst => inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                               inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late &&
                               inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Refunded)
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            if (!unpaid.Any())
                return null;

            // Step 2: find the NEXT upcoming unpaid installment
            var nextUpcoming = unpaid.FirstOrDefault(i => i.Installment_DueDate >= now);

            // Step 3: allowed InstallmentNos
            int maxAllowedInstallmentNo = nextUpcoming?.InstallmentNo ?? unpaid.Last().InstallmentNo;

            // Step 4: final list (exclude future installments)
            var effectiveInstallments = unpaid
                .Where(i => i.InstallmentNo <= maxAllowedInstallmentNo)
                .ToList();

            decimal arrearsBase = 0m;
            decimal notYetDueBase = 0m;
            decimal totalLateInterest = 0m;
            decimal totalOverPayment = 0m;

            decimal paidAgainstArrears = 0m;
            decimal paidAgainstLateInterest = 0m;
            decimal paidAgainstNotYetDue = 0m;

            foreach (var inst in effectiveInstallments)
            {
                decimal unpaidBase = inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase;
                if (unpaidBase < 0) unpaidBase = 0;

                // Track historical payments
                paidAgainstArrears += inst.AmountPaid_AgainstBase;
                paidAgainstLateInterest += inst.AmountPaid_AgainstLateInterest;

                // Track how much was paid toward the not-yet-due installment
                if (inst.Installment_DueDate >= now)
                    paidAgainstNotYetDue += inst.AmountPaid_AgainstBase;

                totalLateInterest += inst.LateInterest;
                totalOverPayment += inst.OverPaymentCarriedFromPreviousInstallment;

                // Categorize base amounts
                if (inst.Installment_DueDate < now)
                    arrearsBase += unpaidBase;
                else
                    notYetDueBase += unpaidBase;
            }

            // Apply paying pattern
            ApplyPayingPattern(
                ref arrearsBase,
                ref totalLateInterest,
                ref notYetDueBase,
                ref totalOverPayment
            );

            decimal payableSettlement =
                arrearsBase +
                totalLateInterest +
                notYetDueBase -
                totalOverPayment;

            if (payableSettlement < 0)
                payableSettlement = 0;

            return new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = effectiveInstallments.First().Bnpl_PlanID,
                CurrentInstallmentNo = effectiveInstallments.First().InstallmentNo,

                Total_InstallmentBaseArrears = arrearsBase,
                NotYetDueCurrentInstallmentBaseAmount = notYetDueBase,
                Total_LateInterest = totalLateInterest,
                Total_OverpaymentCarriedFromPrevious = totalOverPayment,

                Paid_AgainstTotalArrears = paidAgainstArrears,
                Paid_AgainstTotalLateInterest = paidAgainstLateInterest,
                Paid_AgainstNotYetDueCurrentInstallmentBaseAmount = paidAgainstNotYetDue,

                Total_PayableSettlement = payableSettlement,
                IsLatest = true
            };
        }

        //Helper : MarkPreviousSnapshotsOfPlanAsObsoleteAsync
        private void MarkPreviousSnapshotsOfPlanAsObsoleteAsync(BNPL_PLAN existingPlan)
        {
            var snapshots = existingPlan.BNPL_PlanSettlementSummaries;

            if (snapshots.Any())
            {
                foreach (var s in snapshots)
                {
                    s.IsLatest = false;
                    s.Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Obsolete;
                }
            }
        }

        //Helper : ApplyPayingPattern
        private void ApplyPayingPattern(
            ref decimal arrearsBase,
            ref decimal lateInterest,
            ref decimal notYetDueBase,
            ref decimal overPayment)
        {
            if (overPayment > 0)
            {
                decimal reduce = Math.Min(overPayment, arrearsBase);
                arrearsBase -= reduce;
                overPayment -= reduce;
            }

            if (overPayment > 0)
            {
                decimal reduce = Math.Min(overPayment, lateInterest);
                lateInterest -= reduce;
                overPayment -= reduce;
            }

            if (overPayment > 0)
            {
                decimal reduce = Math.Min(overPayment, notYetDueBase);
                notYetDueBase -= reduce;
                overPayment -= reduce;
            }

            // Whatever remains stays as overpayment
        }
    }
}