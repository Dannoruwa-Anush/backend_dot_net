using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Project_Enums;

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
            _repository = repository;
            _bnpl_PlanTypeRepository = bnpl_PlanTypeRepository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_PLAN>> GetAllAsync() =>
            await _repository.GetAllAsync();

        public async Task<BNPL_PLAN?> GetByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<BNPL_PLAN> AddAsync(BNPL_PLAN bNPL_Plan)
        {
            await _repository.AddAsync(bNPL_Plan);

            _logger.LogInformation("BNPL plan created: Id={Id}", bNPL_Plan.Bnpl_PlanID);
            return bNPL_Plan;
        }

        public async Task<BNPL_PLAN?> UpdateAsync(int id, BnplStatusEnum newStatus)
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
            var principal = request.OrderAmount - request.InitialPayment;
            var interestAmount = principal * (planType.InterestRate / 100);
            var totalPayable = principal + interestAmount;
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

        public async Task<PaginationResultDto<BNPL_PLAN>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? planStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, planStatusId, searchKey);
        }
    }
}