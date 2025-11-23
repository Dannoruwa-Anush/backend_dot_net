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
        public async Task<BNPL_PlanSettlementSummary> BuildSettlementGenerateRequestAsync(int planId)
        {
            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Build the new staged settlement snapshot
            var snapshot = await BuildSettlementSnapshotAsync(planId, today);

            // Mark previously-latest summaries as not latest (staged only)
            await MarkOldSnapshotsAsync(planId);

            await _repository.AddAsync(snapshot);

            _logger.LogInformation("Bnpl latest installment snapshot created: SettlementID={SettlementId}, PlanId={PlanId}", snapshot.SettlementID, snapshot.Bnpl_PlanID);
            return snapshot;
        }

        // Helper method : Create snapshot
        private async Task<BNPL_PlanSettlementSummary> BuildSettlementSnapshotAsync(int planId, DateTime asOfDate)
        {
            var unsettled = await GetUnsettledInstallmentsAsync(planId, asOfDate);

            var totals = CalculateSettlementValues(unsettled);

            return new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = planId,
                CurrentInstallmentNo = unsettled.Max(i => i.InstallmentNo),
                InstallmentBaseAmount = totals.TotalBaseAmount,
                TotalCurrentLateInterest = totals.TotalLateInterest,
                TotalCurrentOverPayment = totals.TotalOverPayment,
                TotalCurrentArrears = totals.TotalArrears,
                TotalPayableSettlement = totals.TotalPayable,

                Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Active,
                IsLatest = true,
            };
        }

        // Helper method : Get All Unsettled Installments
        private async Task<List<BNPL_Installment>> GetUnsettledInstallmentsAsync(int planId, DateTime asOfDate)
        {
            var unsettled = await _bNPL_InstallmentRepository
                .GetAllUnsettledInstallmentUpToDateAsync(planId, asOfDate);

            if (unsettled == null || !unsettled.Any())
                throw new Exception("No unsettled installments found");

            return unsettled.ToList();
        }

        // Helper method : Get accumulated settlements
        private (decimal TotalBaseAmount, decimal TotalLateInterest, decimal TotalOverPayment, decimal TotalArrears, decimal TotalPayable) CalculateSettlementValues(List<BNPL_Installment> installments)
        {
            var baseAmount = installments.Sum(i => i.Installment_BaseAmount);
            var lateInterest = installments.Sum(i => i.LateInterest);
            var overPayment = installments.Sum(i => i.OverPaymentCarried);

            // Arrears = Remaining base + arrears carried (late not included)
            var arrears = installments.Sum(i => i.RemainingBalance);

            // Total amount currently payable
            var payable = arrears + lateInterest - overPayment;

            return (baseAmount, lateInterest, overPayment, arrears, payable);
        }

        // Helper method : Mark previous snapshot as IsLatested = false
        private async Task MarkOldSnapshotsAsync(int planId)
        {
            // Only stage updates, no SaveChanges inside
            await _repository.MarkPreviousSnapshotsAsNotLatestAsync(planId);
        }
    }
}