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

        // Constructor
        public BNPL_PlanSettlementSummaryServiceImpl(IBNPL_PlanSettlementSummaryRepository repository, IBNPL_InstallmentRepository bNPL_InstallmentRepository, ICustomerOrderRepository customerOrderRepository)
        {
            // Dependency injection
            _repository = repository;
            _bNPL_InstallmentRepository = bNPL_InstallmentRepository;
            _customerOrderRepository = customerOrderRepository;
        }

        //CRUD operations
        public async Task<BNPL_PlanSettlementSummary> AddBNPL_PlanAsync(BNPL_PlanSettlementSummary snapshot)
        {
            // Mark old snapshots
            await _repository.MarkPreviousSnapshotsAsNotLatestAsync(snapshot.Bnpl_PlanID);

            // Insert new snapshot
            await _repository.AddAsync(snapshot);

            return snapshot;
        }

        //Custom Query Operations
        public async Task<BNPL_PlanSettlementSummary> GenerateSettlementAsync(int planId)
        {
            var today = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);

            // Step 1: Build the settlement snapshot
            var snapshot = await BuildSettlementSummaryAsync(planId, today);

            if (snapshot == null)
                throw new Exception("Settlement snapshot is null");

            // Step 2: Save to DB
            return await AddBNPL_PlanAsync(snapshot);
        }

        // Helper method : BuildSettlementSummaryAsync
        private async Task<BNPL_PlanSettlementSummary> BuildSettlementSummaryAsync(int planId, DateTime asOfDate)
        {
            // Get all unsettled installments up to date
            var unsettled = await _bNPL_InstallmentRepository
                .GetAllUnsettledInstallmentUpToDateAsync(planId, asOfDate);

            if (unsettled == null || !unsettled.Any())
                throw new Exception("Unsettled installments not found");

            // ---------- ACCUMULATIONS ----------
            var totalBaseAmount = unsettled.Sum(i => i.Installment_BaseAmount);
            var totalLateInterest = unsettled.Sum(i => i.LateInterest);
            var totalOverPayment = unsettled.Sum(i => i.OverPaymentCarried);

            var totalArrears = unsettled.Sum(i => (i.TotalDueAmount - i.AmountPaid));

            var totalPayable = totalArrears + totalLateInterest - totalOverPayment;

            var currentInstallmentNo = unsettled.Max(i => i.InstallmentNo);

            // ---------- RETURN POPULATED SNAPSHOT MODEL ----------
            return new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = planId,
                CurrentInstallmentNo = currentInstallmentNo,
                TotalCurrentArrears = totalArrears,
                TotalCurrentLateInterest = totalLateInterest,
                InstallmentBaseAmount = totalBaseAmount,
                TotalCurrentOverPayment = totalOverPayment,
                TotalPayableSettlement = totalPayable,
                Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Active,
                IsLatest = true,
            };
        }

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
    }
}