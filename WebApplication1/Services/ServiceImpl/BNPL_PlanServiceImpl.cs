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
        private readonly ICustomerOrderRepository _customerOrderRepository;

        //logger: for auditing
        private readonly ILogger<BNPL_PlanServiceImpl> _logger;

        // Constructor
        public BNPL_PlanServiceImpl(IBNPL_PlanRepository repository, IBNPL_PlanTypeRepository bnpl_PlanTypeRepository, ICustomerOrderRepository customerOrderRepository, ILogger<BNPL_PlanServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _bnpl_PlanTypeRepository = bnpl_PlanTypeRepository;
            _customerOrderRepository = customerOrderRepository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_PLAN>> GetAllBNPL_PlansAsync() =>
            await _repository.GetAllWithBnplPlanAsync();

        public async Task<BNPL_PLAN?> GetBNPL_PlanByIdAsync(int id) =>
            await _repository.GetWithPlanTypeCustomerDetailsByIdAsync(id);

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, planStatusId, searchKey);
        }

        public async Task<BNPL_PLAN?> GetByOrderIdAsync(int OrderId) =>
            await _repository.GetByOrderIdAsync(OrderId);

        public async Task<BNPL_PLAN?> GetByPlanTypeIdAsync(int planTypeId) =>
            await _repository.GetByPlanTypeIdAsync(planTypeId);    

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

            _logger.LogInformation("BNPL Calculation done for PlanType={Plan}, PrincipalAmount ={PrincipalAmount}, Installments={Count}, Rate={Rate}", planType.Bnpl_PlanTypeName, principalAmount, request.InstallmentCount, planType.InterestRate);

            return new BNPLInstallmentCalculatorResponseDto
            {
                InterestRate = planType.InterestRate,
                LatePayInterestRate = planType.LatePayInterestRatePerDay,
                PlanTypeName = planType.Bnpl_PlanTypeName,
                Description = planType.Bnpl_Description,
                AmountPerInstallment = Math.Round(monthlyInstallment, 2),
                TotalPayable = Math.Round(totalRepaymentAmount, 2),
                TotalInterestAmount = Math.Round(totalInterestAmount, 2)
            };
        }

        //Shared Internal Operations Used by Multiple Repositories
        public async Task<BNPL_PLAN> BuildBnpl_PlanAddRequestAsync(BNPL_PLAN bNPL_Plan)
        {
            // Validate inputs
            if (bNPL_Plan.Bnpl_TotalInstallmentCount <= 0)
                throw new Exception("BNPL plan must have at least one installment.");

            var planType = await _bnpl_PlanTypeRepository.GetByIdAsync(bNPL_Plan.Bnpl_PlanTypeID);
            if (planType == null)
                throw new Exception("Invalid BNPL plan type.");

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
            int freeTrialDays = BnplSystemConstants.FreeTrialPeriodDays;
            int daysPerInstallment = planType.Bnpl_DurationDays;

            bNPL_Plan.Bnpl_RemainingInstallmentCount = bNPL_Plan.Bnpl_TotalInstallmentCount;
            bNPL_Plan.Bnpl_StartDate = now;
            bNPL_Plan.Bnpl_NextDueDate = now.AddDays(freeTrialDays + daysPerInstallment);
            bNPL_Plan.Bnpl_Status = BnplStatusEnum.Active;

            await _repository.AddAsync(bNPL_Plan);
            _logger.LogInformation("Bnpl plan created: Id={Id}, OrderId={OrderId}", bNPL_Plan.Bnpl_PlanID, bNPL_Plan.OrderID);
            return bNPL_Plan;
        }
    }
}