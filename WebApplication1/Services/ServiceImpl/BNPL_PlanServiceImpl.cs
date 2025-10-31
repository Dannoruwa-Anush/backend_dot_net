using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_PlanServiceImpl : IBNPL_PlanService
    {
        private readonly IBNPL_PlanRepository _repository;
        private readonly IBNPL_PlanTypeRepository _bnpl_PlanTypeRepository;

        //logger: for auditing
        private readonly ILogger<BNPL_PlanServiceImpl> _logger;

        public BNPL_PlanServiceImpl(IBNPL_PlanRepository repository, IBNPL_PlanTypeRepository bnpl_PlanTypeRepository, ILogger<BNPL_PlanServiceImpl> logger)
        {
            // Constructor
            _repository              = repository;
            _bnpl_PlanTypeRepository = bnpl_PlanTypeRepository;
            _logger                  = logger;
        }
        //CRUD operations


        //Custom Query Operations
        public async Task<BNPLInstallmentCalculatorResponseDto> CalculateAmountPerInstallmentAsync(BNPLInstallmentCalculatorRequestDto request)
        {
            var planType = await _bnpl_PlanTypeRepository.GetByIdAsync(request.Bnpl_PlanTypeID);
            if (planType == null)
                throw new Exception("Invalid BNPL plan type.");

            if (request.InstallmentCount <= 0)
                throw new Exception("Installment count must be greater than zero.");

            if (request.OrderAmount <= request.InitialPayment)
                throw new Exception("Initial payment must be less than total order amount.");

            // Core calculation
            var principal      = request.OrderAmount - request.InitialPayment;
            var interestAmount = principal * (planType.InterestRate / 100);
            var totalPayable   = principal + interestAmount;
            var perInstallment = totalPayable / request.InstallmentCount;

            _logger.LogInformation("BNPL Calculation done for PlanType={Plan}, Principal={Principal}, Installments={Count}, Rate={Rate}", planType.Bnpl_PlanTypeName, principal, request.InstallmentCount, planType.InterestRate
            );

            return new BNPLInstallmentCalculatorResponseDto
            {
                AmountPerInstallment = Math.Round(perInstallment, 2),
                TotalPayable = Math.Round(totalPayable, 2),
                InterestRate = planType.InterestRate,
                LatePayInterestRate = planType.LatePayInterestRate,
                PlanTypeName = planType.Bnpl_PlanTypeName,
                Description = planType.Bnpl_Description
            };
        }
    }
}