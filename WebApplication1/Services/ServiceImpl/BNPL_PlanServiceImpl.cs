using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
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

        //logger: for auditing
        private readonly ILogger<BNPL_PlanServiceImpl> _logger;

        // Constructor
        public BNPL_PlanServiceImpl(IBNPL_PlanRepository repository, IBNPL_PlanTypeRepository bnpl_PlanTypeRepository, IBNPL_InstallmentRepository bnpl_installmentRepository, ICustomerOrderRepository customerOrderRepository, ILogger<BNPL_PlanServiceImpl> logger)
        {
            // Dependency injection
            _repository                 = repository;
            _bnpl_PlanTypeRepository    = bnpl_PlanTypeRepository;
            _bnpl_installmentRepository = bnpl_installmentRepository;
            _customerOrderRepository    = customerOrderRepository;
            _logger                     = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_PLAN>> GetAllBNPL_PlansAsync() =>
            await _repository.GetAllAsync();

        public async Task<BNPL_PLAN?> GetBNPL_PlanByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<BNPL_PLAN> AddBNPL_PlanAsync(BNPL_PLAN bNPL_Plan)
        {
            using var transaction = await _repository.BeginTransactionAsync();

            try
            {
                if (bNPL_Plan.Bnpl_TotalInstallmentCount <= 0)
                    throw new Exception("BNPL plan must have at least one installment.");

                var planType = await _bnpl_PlanTypeRepository.GetByIdAsync(bNPL_Plan.Bnpl_PlanTypeID);
                if (planType == null)
                    throw new Exception("Invalid BNPL plan type.");

                await _repository.AddAsync(bNPL_Plan);
                await _repository.SaveChangesAsync();

                int totalDays = planType.Bnpl_DurationDays;
                int installmentCount = bNPL_Plan.Bnpl_TotalInstallmentCount;
                double daysPerInstallment = (double)totalDays / installmentCount;

                //system const
                int freeTrialDays = BnplSystemConstants.FreeTrialPeriodDays;

                var installments = new List<BNPL_Installment>();
                var startDate = bNPL_Plan.Bnpl_StartDate;

                for (int i = 1; i <= installmentCount; i++)
                {
                    //First installment should be due after a trial period
                    //Then subsequent installments follow the normal interval.
                    var dueDate = startDate.AddDays(freeTrialDays + (daysPerInstallment * (i - 1)));

                    installments.Add(new BNPL_Installment
                    {
                        Bnpl_PlanID = bNPL_Plan.Bnpl_PlanID,
                        BNPL_PLAN = bNPL_Plan,
                        InstallmentNo = i,
                        Installment_BaseAmount = bNPL_Plan.Bnpl_AmountPerInstallment,
                        Installment_DueDate = dueDate,
                        TotalDueAmount = bNPL_Plan.Bnpl_AmountPerInstallment,
                        CreatedAt = DateTime.UtcNow,
                        Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Pending
                    });
                }

                await _bnpl_installmentRepository.AddRangeAsync(installments);
                await _repository.SaveChangesAsync();

                bNPL_Plan.Bnpl_NextDueDate = installments.First().Installment_DueDate;
                await _repository.UpdateAsync(bNPL_Plan.Bnpl_PlanID, bNPL_Plan);
                await _repository.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "BNPL plan created: Id={Id}, {Count} installments scheduled, trial={Trial} days before first installment.",
                    bNPL_Plan.Bnpl_PlanID,
                    installments.Count,
                    freeTrialDays
                );

                return bNPL_Plan;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create BNPL plan with installments.");
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
            var principal = customerOrder.TotalAmount - request.InitialPayment;
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
    }
}