using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.Common;
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

        // logger: for auditing
        private readonly ILogger<BNPL_PlanSettlementSummaryServiceImpl> _logger;

        // Constructor
        public BNPL_PlanSettlementSummaryServiceImpl(IBNPL_PlanSettlementSummaryRepository repository, ILogger<BNPL_PlanSettlementSummaryServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
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
        public BnplLatestSnapshotSettledResultDto BuildBNPL_PlanLatestSettlementSummaryUpdateRequest(CustomerOrder existingOrder, decimal paymentAmount)
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


        //-----------------[Start: Settlement Generation]---------------
        public BNPL_PlanSettlementSummary? BuildSettlementGenerateRequestForPlan(BNPL_PLAN existingPlan)
        {
            var installments = existingPlan.BNPL_Installments;

            if (installments == null || installments.Count == 0)
                return null;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Filter unpaid or partially paid installments
            var unpaidInstallments = installments
                .Where(i => i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                            i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late &&
                            i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Refunded)
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            if (!unpaidInstallments.Any())
                return null;

            // Using helper method segregate overdue and nearest upcoming
            var segregation = SegregateInstallments(unpaidInstallments, now);

            var overdueInstallments = segregation.OverdueInstallments;
            var nearestUpcomingInstallment = segregation.NearestUpcomingInstallment;

            // Accumulators : Overdue
            decimal arrearsBase = overdueInstallments.Sum(i => Math.Max(0, i.Installment_BaseAmount - i.AmountPaid_AgainstBase));

            decimal totalLateInterest = overdueInstallments.Sum(i => Math.Max(0, i.LateInterest - i.AmountPaid_AgainstLateInterest))
                                      + (nearestUpcomingInstallment != null
                                         ? Math.Max(0, nearestUpcomingInstallment.LateInterest - nearestUpcomingInstallment.AmountPaid_AgainstLateInterest)
                                         : 0m);

            //Upcoming nearest installment                             
            decimal notYetDueBase = nearestUpcomingInstallment != null
                            ? Math.Max(0, nearestUpcomingInstallment.Installment_BaseAmount - nearestUpcomingInstallment.AmountPaid_AgainstBase)
                            : 0m;

            // Reporting-only
            decimal paidAgainstArrears = overdueInstallments.Sum(i => i.AmountPaid_AgainstBase);

            decimal paidAgainstLateInterest = overdueInstallments.Sum(i => i.AmountPaid_AgainstLateInterest)
                                           + (nearestUpcomingInstallment?.AmountPaid_AgainstLateInterest ?? 0m);

            decimal paidAgainstNotYetDue = nearestUpcomingInstallment?.AmountPaid_AgainstBase ?? 0m;

            //total settlement
            decimal totalPayable = arrearsBase + notYetDueBase + totalLateInterest;

            //Using helper method, mark previous snapshots as obsolate
            MarkLatestSnapshotObsolete(existingPlan);

            // Pick first effective installment for reference
            var firstEffectiveInstallment = overdueInstallments.FirstOrDefault() ?? nearestUpcomingInstallment!;

            return new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = firstEffectiveInstallment.Bnpl_PlanID,
                CurrentInstallmentNo = firstEffectiveInstallment.InstallmentNo,

                Total_InstallmentBaseArrears = arrearsBase,
                Total_LateInterest = totalLateInterest,
                NotYetDueCurrentInstallmentBaseAmount = notYetDueBase,

                Paid_AgainstTotalArrears = paidAgainstArrears,
                Paid_AgainstNotYetDueCurrentInstallmentBaseAmount = paidAgainstNotYetDue,
                Paid_AgainstTotalLateInterest = paidAgainstLateInterest,

                Total_PayableSettlement = totalPayable,

                IsLatest = true,
                Bnpl_PlanSettlementSummaryRef = $"SNP-{firstEffectiveInstallment.Bnpl_PlanID}-for-{firstEffectiveInstallment.InstallmentNo}-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}"
            };
        }

        // Helper record to return segregation results (DTO)
        private record InstallmentSegregationResult(
            List<BNPL_Installment> OverdueInstallments,
            BNPL_Installment? NearestUpcomingInstallment
        );

        // Helper method to segregate installments
        private InstallmentSegregationResult SegregateInstallments(List<BNPL_Installment> unpaidInstallments, DateTime now)
        {
            if (unpaidInstallments == null || unpaidInstallments.Count == 0)
                return new InstallmentSegregationResult(new List<BNPL_Installment>(), null);

            // Overdue installments
            var overdueInstallments = unpaidInstallments
                .Where(i => i.Installment_DueDate < now)
                .OrderBy(i => i.Installment_DueDate)
                .ThenBy(i => i.InstallmentNo)
                .ToList();

            // Nearest upcoming installment
            var nearestUpcomingInstallment = unpaidInstallments
                .Where(i => i.Installment_DueDate >= now)
                .OrderBy(i => i.InstallmentNo) // Ensure the smallest InstallmentNo >= now
                .FirstOrDefault();

            return new InstallmentSegregationResult(overdueInstallments, nearestUpcomingInstallment);
        }

        // Helper to mark last snapshot as obsolete
        private void MarkLatestSnapshotObsolete(BNPL_PLAN existingBnplPlan)
        {
            if (existingBnplPlan == null)
                throw new ArgumentNullException(nameof(existingBnplPlan));

            var latestSnapshot = existingBnplPlan.BNPL_PlanSettlementSummaries
                .FirstOrDefault(s => s.IsLatest);

            if (latestSnapshot == null)
                return;

            latestSnapshot.IsLatest = false;
            latestSnapshot.Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Obsolete;
        }
        //-----------------[End: Settlement Generation]---------------


        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_PlanSettlementSummary>> GetAllLatestSnapshotWithPaginationAsync(int pageNumber, int pageSize, string? searchKey = null)
        {
            return await _repository.GetAllLatestSnapshotWithPaginationAsync(pageNumber, pageSize, searchKey);
        }

        public void ApplyFrozenSettlementSnapshot(CustomerOrder order, BnplLatestSnapshotSettledResultDto frozenSnapshot)
        {
            var plan = order.BNPL_PLAN
                ?? throw new Exception("BNPL plan not found");

            var latestSnapshot = plan.BNPL_PlanSettlementSummaries
                .FirstOrDefault(s => s.IsLatest)
                ?? throw new Exception("Latest settlement snapshot not found");

            // APPLY — NO CALCULATION
            latestSnapshot.Paid_AgainstTotalArrears += frozenSnapshot.TotalPaidArrears;

            latestSnapshot.Paid_AgainstTotalLateInterest += frozenSnapshot.TotalPaidLateInterest;

            latestSnapshot.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount += frozenSnapshot.TotalPaidCurrentInstallmentBase;

            var totalApplied = frozenSnapshot.TotalPaidArrears + frozenSnapshot.TotalPaidLateInterest + frozenSnapshot.TotalPaidCurrentInstallmentBase;

            latestSnapshot.Total_PayableSettlement = Math.Max(latestSnapshot.Total_PayableSettlement - totalApplied, 0m);

            latestSnapshot.Total_OverpaymentCarriedToNext += frozenSnapshot.OverPaymentCarriedToNextInstallment;

            if (latestSnapshot.Total_PayableSettlement == 0)
            {
                latestSnapshot.Bnpl_PlanSettlementSummary_PaymentStatus = Bnpl_PlanSettlementSummary_PaymentStatusEnum.Settled;
            }

            _logger.LogInformation("Applied frozen settlement snapshot to OrderId={OrderId}", order.OrderID);
        }
    }
}