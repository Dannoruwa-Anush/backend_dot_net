using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_PlanServiceImpl : IBNPL_PlanService
    {
        private readonly IBNPL_PlanRepository _repository;
        private readonly IBNPL_PlanTypeRepository _bnpl_PlanTypeRepository;
        private readonly IBNPL_InstallmentRepository _bnpl_installmentRepository;
        private readonly ICustomerOrderRepository _customerOrderRepository;
        private readonly IBNPL_PlanSettlementSummaryService _bnpl_planSettlementSummaryService;

        //logger: for auditing
        private readonly ILogger<BNPL_PlanServiceImpl> _logger;

        // Constructor
        public BNPL_PlanServiceImpl(IBNPL_PlanRepository repository, IBNPL_PlanTypeRepository bnpl_PlanTypeRepository, IBNPL_InstallmentRepository bnpl_installmentRepository, ICustomerOrderRepository customerOrderRepository, IBNPL_PlanSettlementSummaryService bnpl_planSettlementSummaryService, ILogger<BNPL_PlanServiceImpl> logger)
        {
            // Dependency injection
            _repository                         = repository;
            _bnpl_PlanTypeRepository            = bnpl_PlanTypeRepository;
            _bnpl_installmentRepository         = bnpl_installmentRepository;
            _customerOrderRepository            = customerOrderRepository;
            _bnpl_planSettlementSummaryService  = bnpl_planSettlementSummaryService;
            _logger                             = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_PLAN>> GetAllBNPL_PlansAsync() =>
            await _repository.GetAllAsync();

        public async Task<BNPL_PLAN?> GetBNPL_PlanByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<BNPL_PLAN> AddBNPL_PlanAsync(BNPL_PLAN bNPL_Plan)
        {
            await using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                // Validate installment count
                if (bNPL_Plan.Bnpl_TotalInstallmentCount <= 0)
                    throw new Exception("BNPL plan must have at least one installment.");

                // Validate BNPL plan type
                var planType = await _bnpl_PlanTypeRepository.GetByIdAsync(bNPL_Plan.Bnpl_PlanTypeID);
                if (planType == null)
                    throw new Exception("Invalid BNPL plan type.");

                var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
                int freeTrialDays = BnplSystemConstants.FreeTrialPeriodDays;

                int installmentCount = bNPL_Plan.Bnpl_TotalInstallmentCount;
                int daysPerInstallment = planType.Bnpl_DurationDays;

                // Prepare BNPL Plan
                bNPL_Plan.Bnpl_RemainingInstallmentCount = installmentCount;
                bNPL_Plan.Bnpl_StartDate = now;
                bNPL_Plan.Bnpl_NextDueDate = now.AddDays(freeTrialDays + daysPerInstallment);
                bNPL_Plan.Bnpl_Status = BnplStatusEnum.Active;

                // Save plan
                await _repository.AddAsync(bNPL_Plan);
                await _repository.SaveChangesAsync();

                // Create installment list
                var installments = new List<BNPL_Installment>(installmentCount);

                for (int i = 1; i <= installmentCount; i++)
                {
                    var dueDate = now.AddDays(freeTrialDays + (daysPerInstallment * (i - 1)));

                    installments.Add(new BNPL_Installment
                    {
                        Bnpl_PlanID = bNPL_Plan.Bnpl_PlanID,
                        InstallmentNo = i,
                        Installment_BaseAmount = bNPL_Plan.Bnpl_AmountPerInstallment,
                        Installment_DueDate = dueDate,
                        TotalDueAmount = bNPL_Plan.Bnpl_AmountPerInstallment,
                        CreatedAt = now,
                        Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Pending
                    });
                }

                // Add installments in a single batch
                await _bnpl_installmentRepository.AddRangeAsync(installments);

                // Create Settlement summuary (Snapshot)
                await _bnpl_planSettlementSummaryService.GenerateSettlementAsync(bNPL_Plan.Bnpl_PlanID);

                await _repository.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "BNPL Plan Created: ID={Id}, {Count} installments, FreeTrial={TrialDays} days",
                    bNPL_Plan.Bnpl_PlanID,
                    installments.Count,
                    freeTrialDays
                );

                return bNPL_Plan;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create BNPL plan.");
                throw;
            }
        }

        public async Task<BNPL_PLAN?> UpdateBNPL_PlanAsync(int id, BnplStatusEnum newStatus)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("BNPL plan not found.");

            // Validate: cancellation only before shipping or within 14 days after delivery
            if (newStatus == BnplStatusEnum.Refunded || newStatus == BnplStatusEnum.Cancelled)
            {
                var order = existing.CustomerOrder;

                if (order == null)
                    throw new Exception("Associated order not found.");

                if (order.ShippedDate != null)
                {
                    // Check if it's within 14 days of delivery
                    var deliveredAt = order.DeliveredDate;
                    if (deliveredAt != null && DateTime.UtcNow > deliveredAt.Value.AddDays(14))
                        throw new Exception("Cancellation not allowed after 14 days of delivery.");
                }
            }

            existing.Bnpl_Status = newStatus;
            return await _repository.UpdateAsync(id, existing);
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, planStatusId, searchKey);
        }

        //calculator
        public async Task<BNPLInstallmentCalculatorResponseDto> CalculateBNPL_PlanAmountPerInstallmentAsync(BNPLInstallmentCalculatorRequestDto request)
        {
            var planType = await _bnpl_PlanTypeRepository.GetByIdAsync(request.Bnpl_PlanTypeID);
            if (planType == null)
                throw new Exception("Invalid BNPL plan type.");

            if (request.InstallmentCount <= 0)
                throw new Exception("Installment count must be greater than zero.");

            var customerOrder = await _customerOrderRepository.GetByIdAsync(request.OrderID);
            if (customerOrder == null)
                throw new Exception("Customer order not found");

            if (customerOrder.TotalAmount <= request.InitialPayment)
                throw new Exception("Initial payment must be less than total order amount.");

            // Core calculation
            decimal principalAmount = customerOrder.TotalAmount - request.InitialPayment;
            decimal monthlyInterestRate = planType.InterestRate / 100m;
            int installmentCount = request.InstallmentCount;

            // Amortized monthly payment formula
            /*
            Note :
                Monthly payment is fixed : makes budgeting predictable
                Interest portion decreases over time : because it’s always calculated on the remaining balance
                Principal portion increases over time : so by the last installment, almost the entire payment goes to principal
            */
            decimal monthlyInstallment = (monthlyInterestRate * principalAmount) / (1 - (decimal)Math.Pow((double)(1 + monthlyInterestRate), -installmentCount));

            decimal totalRepaymentAmount = monthlyInstallment * installmentCount;
            decimal totalInterestAmount = totalRepaymentAmount - principalAmount;

            _logger.LogInformation("BNPL Calculation done for PlanType={Plan}, PrincipalAmount ={PrincipalAmount}, Installments={Count}, Rate={Rate}", planType.Bnpl_PlanTypeName, principalAmount, request.InstallmentCount, planType.InterestRate
            );

            return new BNPLInstallmentCalculatorResponseDto
            {
                InterestRate = planType.InterestRate,
                LatePayInterestRate = planType.LatePayInterestRate,
                PlanTypeName = planType.Bnpl_PlanTypeName,
                Description = planType.Bnpl_Description,
                AmountPerInstallment = Math.Round(monthlyInstallment, 2),
                TotalPayable = Math.Round(totalRepaymentAmount, 2),
                TotalInterestAmount = Math.Round(totalInterestAmount, 2)
            };
        }
    }
}