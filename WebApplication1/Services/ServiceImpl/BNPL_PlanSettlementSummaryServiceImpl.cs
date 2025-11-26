using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
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
        public async Task<BNPL_PlanSettlementSummary?> BuildSettlementGenerateRequestForPlanAsync(List<BNPL_Installment> plan_Installments)
        {
            var unsettledInstallments = plan_Installments
                .Where(i =>
                    i.RemainingBalance > 0 ||
                    i.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.Pending ||
                    i.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.PartiallyPaid_OnTime ||
                    i.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.PartiallyPaid_Late ||
                    i.Bnpl_Installment_Status == BNPL_Installment_StatusEnum.Overdue
                )
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            if (!unsettledInstallments.Any())
                return null;

            int planId = unsettledInstallments.First().Bnpl_PlanID;

            // Mark previous snapshots as not latest
            var existingSnapshots = await _repository.GetAllByPlanIdAsync(planId);

            foreach (var snapshotItem in existingSnapshots)
                snapshotItem.IsLatest = false;

            // Calculate totals
            var totals = CalculateSettlementValues(unsettledInstallments);

            // Create new snapshot
            var snapshot = new BNPL_PlanSettlementSummary
            {
                Bnpl_PlanID = planId,
                CurrentInstallmentNo = unsettledInstallments.Max(i => i.InstallmentNo),
                InstallmentBaseAmount = totals.TotalBaseAmount,
                TotalCurrentLateInterest = totals.TotalLateInterest,
                TotalCurrentOverPayment = totals.TotalOverPayment,
                TotalCurrentArrears = totals.TotalArrears,
                TotalPayableSettlement = totals.TotalPayable,
                Bnpl_PlanSettlementSummary_Status = BNPL_PlanSettlementSummary_StatusEnum.Active,
                IsLatest = true,
            };

            _logger.LogInformation("Snapshot created for Plan={Bnpl_PlanID}", planId);
            return snapshot;
        }

        //Helper method : to calculate accumulated values
        private (decimal TotalBaseAmount, decimal TotalLateInterest, decimal TotalOverPayment, decimal TotalArrears, decimal TotalPayable) CalculateSettlementValues(List<BNPL_Installment> installments)
        {
            var baseAmount = installments.Sum(i => i.Installment_BaseAmount);
            var lateInterest = installments.Sum(i => i.LateInterest);
            var overPayment = installments.Sum(i => i.OverPaymentCarriedFromPreviousInstallment);
            var arrears = installments.Sum(i => i.RemainingBalance);
            var payable = arrears + lateInterest - overPayment;

            return (baseAmount, lateInterest, overPayment, arrears, payable);
        }
    }
}