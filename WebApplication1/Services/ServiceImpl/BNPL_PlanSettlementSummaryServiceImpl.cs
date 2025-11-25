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

        private readonly IBNPL_InstallmentRepository _bNPL_InstallmentRepository;
        private readonly ICustomerOrderRepository _customerOrderRepository;

        // logger: for auditing
        private readonly ILogger<BNPL_PlanSettlementSummaryServiceImpl> _logger;

        // Constructor
        public BNPL_PlanSettlementSummaryServiceImpl(IBNPL_PlanSettlementSummaryRepository repository, IBNPL_InstallmentRepository bNPL_InstallmentRepository, ICustomerOrderRepository customerOrderRepository, ILogger<BNPL_PlanSettlementSummaryServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _bNPL_InstallmentRepository = bNPL_InstallmentRepository;
            _customerOrderRepository = customerOrderRepository;
            _logger = logger;
        }

        //Custom Query Operations
        public Task<BNPL_PlanSettlementSummary?> GetLatestSnapshotWithOrderDetailsAsync(int orderId) =>
            _repository.GetLatestSnapshotWithOrderDetailsAsync(orderId);

        //simulator : Main Driver
        public async Task<BnplSnapshotPayingSimulationResultDto> SimulateBnplPlanSettlementAsync(BnplSnapshotPayingSimulationRequestDto request)
        {
            // Load the customer order
            var order = await _customerOrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new Exception("Customer order not found.");

            var planId = order.BNPL_PLAN!.Bnpl_PlanID;

            // Initialize remaining payment with input
            decimal remainingPayment = request.PaymentAmount;

            var latestSnapshot = await _repository.GetLatestSnapshotAsync(planId);
            if (latestSnapshot == null)
                throw new Exception("Latest snapshot not found");

            return await SimulatePerInstallmentInternalAsync(latestSnapshot, remainingPayment);
        }

        //Helper method : simulator per installment
        private async Task<BnplSnapshotPayingSimulationResultDto> SimulatePerInstallmentInternalAsync(BNPL_PlanSettlementSummary latestSnapshot, decimal paymentAmount)
        {
            return await Task.Run(() =>
            {
                decimal remaining = paymentAmount;
                decimal paidToArrears = 0m;
                decimal paidToInterest = 0m;
                decimal paidToBase = 0m;

                decimal arrears = latestSnapshot.TotalCurrentArrears;
                decimal interest = latestSnapshot.TotalCurrentLateInterest;
                decimal baseAmount = latestSnapshot.InstallmentBaseAmount;
                decimal totalDue = latestSnapshot.TotalPayableSettlement;

                // -----------------------------
                // 1. PAY ARREARS FIRST
                // -----------------------------
                if (arrears > 0 && remaining > 0)
                {
                    paidToArrears = Math.Min(arrears, remaining);
                    remaining -= paidToArrears;
                }

                // -----------------------------
                // 2. PAY LATE INTEREST
                // -----------------------------
                if (interest > 0 && remaining > 0)
                {
                    paidToInterest = Math.Min(interest, remaining);
                    remaining -= paidToInterest;
                }

                // -----------------------------
                // 3. PAY BASE AMOUNT
                // -----------------------------
                if (baseAmount > 0 && remaining > 0)
                {
                    paidToBase = Math.Min(baseAmount, remaining);
                    remaining -= paidToBase;
                }

                // ---------------------------------
                // CALCULATE APPLIED + OVERPAYMENT
                // ---------------------------------
                decimal appliedTotal = paidToArrears + paidToInterest + paidToBase;
                decimal remainingBalance = Math.Max(0, totalDue - appliedTotal);
                decimal overPayment = remaining > 0 ? remaining : 0;

                // ---------------------------------
                // DETERMINE RESULT STATUS
                // ---------------------------------
                string status;

                if (remainingBalance == 0 && overPayment > 0)
                    status = "Overpaid";
                else if (remainingBalance == 0)
                    status = "Fully Settled";
                else
                    status = "Partially Paid";

                return new BnplSnapshotPayingSimulationResultDto
                {
                    InstallmentId = latestSnapshot.CurrentInstallmentNo,
                    PaidToArrears = paidToArrears,
                    PaidToInterest = paidToInterest,
                    PaidToBase = paidToBase,
                    RemainingBalance = remainingBalance,
                    OverPaymentCarried = overPayment,
                    ResultStatus = status
                };
            });
        }

        //Shared Internal Operations Used by Multiple Repositories
        // Single plan (for payment processing)
        public async Task<BNPL_PlanSettlementSummary> BuildSettlementGenerateRequestForPlanAsync(List<BNPL_Installment> plan_Installments)
        {
            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
            
            
            
            return null;
            //return (await BuildSettlementGenerateRequestBatchAsync(new List<int> { planId }, today)).First();
        }

        // Batch version (for late interest updates or bulk operations)
        public async Task<List<BNPL_PlanSettlementSummary>> BuildSettlementGenerateRequestBatchAsync(List<int> planIds, DateTime asOfDate)
        {
            if (planIds == null || !planIds.Any())
                return new List<BNPL_PlanSettlementSummary>();

            // Fetch all unsettled installments for all plans
            var allInstallments = await _bNPL_InstallmentRepository.GetAllUnsettledInstallmentsForPlansAsync(planIds, asOfDate);

            var snapshots = new List<BNPL_PlanSettlementSummary>();

            foreach (var planId in planIds)
            {
                var installments = allInstallments.Where(i => i.Bnpl_PlanID == planId).ToList();
                if (!installments.Any())
                {
                    _logger.LogInformation("No unsettled installments found for PlanId={PlanId}", planId);
                    continue;
                }

                var totals = CalculateSettlementValues(installments);

                var snapshot = new BNPL_PlanSettlementSummary
                {
                    Bnpl_PlanID = planId,
                    CurrentInstallmentNo = installments.Max(i => i.InstallmentNo),
                    InstallmentBaseAmount = totals.TotalBaseAmount,
                    TotalCurrentLateInterest = totals.TotalLateInterest,
                    TotalCurrentOverPayment = totals.TotalOverPayment,
                    TotalCurrentArrears = totals.TotalArrears,
                    TotalPayableSettlement = totals.TotalPayable,
                    Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Active,
                    IsLatest = true,
                };

                snapshots.Add(snapshot);
            }

            if (snapshots.Any())
            {
                // Mark old snapshots as not latest in batch
                await _repository.MarkPreviousSnapshotsAsNotLatestBatchAsync(snapshots.Select(s => s.Bnpl_PlanID).ToList());
                
                await _repository.AddRangeAsync(snapshots);
                _logger.LogInformation("Snapshots created for {Count} plans", snapshots.Count);
            }

            return snapshots;
        }

        private (decimal TotalBaseAmount, decimal TotalLateInterest, decimal TotalOverPayment, decimal TotalArrears, decimal TotalPayable) CalculateSettlementValues(List<BNPL_Installment> installments)
        {
            var baseAmount = installments.Sum(i => i.Installment_BaseAmount);
            var lateInterest = installments.Sum(i => i.LateInterest);
            var overPayment = installments.Sum(i => i.OverPaymentCarried);
            var arrears = installments.Sum(i => i.RemainingBalance);
            var payable = arrears + lateInterest - overPayment;

            return (baseAmount, lateInterest, overPayment, arrears, payable);
        }
    }
}