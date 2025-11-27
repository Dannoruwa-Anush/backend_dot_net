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
        public (BnplLatestSnapshotSettledResultDto, BNPL_PlanSettlementSummary) BuildBNPL_PlanLatestSettlementSummaryUpdateRequestAsync(BNPL_PlanSettlementSummary latestSnapshot, decimal paymentAmount)
        {
            //Call helper method
            (decimal paidArrears, decimal paidInterest, decimal paidBase, decimal remainingBalance, decimal nextSnapshotOverPayment) = AllocatePaymentBuckets(latestSnapshot, paymentAmount);

            // Update only what belongs to this snapshot
            latestSnapshot.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount += paidBase;
            latestSnapshot.Paid_AgainstTotalArrears += paidArrears;
            latestSnapshot.Paid_AgainstTotalLateInterest += paidInterest;

            //???? check
            latestSnapshot.Total_OverpaymentCarriedToNext = nextSnapshotOverPayment;

            var totalSettlement = new BnplLatestSnapshotSettledResultDto
            {
                TotalPaidArrears = paidArrears,
                TotalPaidLateInterest = paidInterest,
                TotalPaidCurrentInstallmentBase = paidBase,
                OverPaymentCarriedToNextInstallment = nextSnapshotOverPayment
            };

            return (totalSettlement, latestSnapshot);
        }

        ///*********************************************** need to check again*************************
        public BNPL_PlanSettlementSummary? BuildSettlementGenerateRequestForPlanAsync(BNPL_PLAN existingPlan, Bnpl_PlanSettlementSummary_TypeEnum SnapshotType)
        {
            var installments = existingPlan.BNPL_Installments;

            if (installments == null || installments.Count == 0)
                return null;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // ---------------------------------------------------------
            // 1. FILTER UNPAID INSTALLMENTS
            // ---------------------------------------------------------
            var unpaid = installments
                .Where(inst => inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                               inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late &&
                               inst.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Refunded)
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            if (!unpaid.Any())
                return null;

            var nextUpcoming = unpaid.FirstOrDefault(i => i.Installment_DueDate >= now);

            int maxAllowedInstallmentNo =
                nextUpcoming?.InstallmentNo ?? unpaid.Last().InstallmentNo;

            var effectiveInstallments = unpaid
                .Where(i => i.InstallmentNo <= maxAllowedInstallmentNo)
                .ToList();

            // ---------------------------------------------------------
            // ACCUMULATORS
            // ---------------------------------------------------------
            decimal arrearsBase = 0m;
            decimal notYetDueBase = 0m;
            decimal totalLateInterest = 0m;
            decimal totalOverPaymentCarriedForward = 0m;

            decimal paidAgainstArrears = 0m;
            decimal paidAgainstLateInterest = 0m;
            decimal paidAgainstNotYetDue = 0m;

            // ---------------------------------------------------------
            // 2. PROCESS EACH INSTALLMENT
            // ---------------------------------------------------------
            foreach (var inst in effectiveInstallments)
            {
                decimal remainingBase = Math.Max(0, inst.Installment_BaseAmount - inst.AmountPaid_AgainstBase);
                decimal remainingInterest = Math.Max(0, inst.LateInterest - inst.AmountPaid_AgainstLateInterest);

                paidAgainstArrears += inst.AmountPaid_AgainstBase;
                paidAgainstLateInterest += inst.AmountPaid_AgainstLateInterest;

                if (inst.Installment_DueDate >= now)
                    paidAgainstNotYetDue += inst.AmountPaid_AgainstBase;

                // APPLY OVERPAYMENT
                decimal carry = inst.OverPaymentCarriedFromPreviousInstallment;

                decimal arrearsPortion = inst.Installment_DueDate < now ? remainingBase : 0m;
                decimal futureBasePortion = inst.Installment_DueDate >= now ? remainingBase : 0m;

                var (paidCarryToArrears, paidCarryToInterest, paidCarryToBase, leftoverCarry)
                    = ApplyPayingFlow(carry, arrearsPortion, remainingInterest, futureBasePortion);

                remainingBase -= paidCarryToArrears + paidCarryToBase;
                remainingInterest -= paidCarryToInterest;

                totalOverPaymentCarriedForward += leftoverCarry;

                if (inst.Installment_DueDate < now)
                    arrearsBase += remainingBase;
                else
                    notYetDueBase += remainingBase;

                totalLateInterest += remainingInterest;
            }

            decimal payableSettlement =
                arrearsBase + notYetDueBase + totalLateInterest -
                (paidAgainstArrears + paidAgainstLateInterest + paidAgainstNotYetDue);

            if (payableSettlement < 0)
                payableSettlement = 0;

            // ---------------------------------------------------------
            // SAFELY Mark previous snapshot obsolete
            // ---------------------------------------------------------
            MarkLatestSnapshotObsolete(existingPlan);

            // ---------------------------------------------------------
            // RETURN SNAPSHOT
            // ---------------------------------------------------------
            return new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = effectiveInstallments.First().Bnpl_PlanID,
                CurrentInstallmentNo = effectiveInstallments.First().InstallmentNo,

                Total_InstallmentBaseArrears = arrearsBase,
                NotYetDueCurrentInstallmentBaseAmount = notYetDueBase,
                Total_LateInterest = totalLateInterest,

                Total_OverpaymentCarriedFromPrevious = totalOverPaymentCarriedForward,

                Paid_AgainstTotalArrears = paidAgainstArrears,
                Paid_AgainstTotalLateInterest = paidAgainstLateInterest,
                Paid_AgainstNotYetDueCurrentInstallmentBaseAmount = paidAgainstNotYetDue,

                Total_PayableSettlement = payableSettlement,
                IsLatest = true,
                Bnpl_PlanSettlementSummary_Type = SnapshotType,
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