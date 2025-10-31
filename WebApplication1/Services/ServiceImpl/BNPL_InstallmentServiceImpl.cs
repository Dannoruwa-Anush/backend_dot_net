using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Project_Enums;
using WebApplication1.Utils.SystemConstants;

namespace WebApplication1.Services.ServiceImpl
{
    public class BNPL_InstallmentServiceImpl : IBNPL_InstallmentService
    {
        private readonly IBNPL_InstallmentRepository _repository;

        //logger: for auditing
        private readonly ILogger<BNPL_InstallmentServiceImpl> _logger;

        // Constructor
        public BNPL_InstallmentServiceImpl(IBNPL_InstallmentRepository repository, ILogger<BNPL_InstallmentServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<BNPL_Installment>> GetAllBNPL_InstallmentsAsync() =>
            await _repository.GetAllAsync();

        public async Task<BNPL_Installment?> GetBNPL_InstallmentByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        //Custom Query Operations
        public async Task<PaginationResultDto<BNPL_Installment>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? bnpl_Installment_StatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);
        }

        public async Task<BNPL_Installment?> CancelInstallmentAsync(int id)
        {
            var installment = await _repository.GetByIdAsync(id)
                ?? throw new Exception("Installment not found.");

            var plan = installment.BNPL_PLAN;
            if (plan?.CustomerOrder == null)
                throw new Exception("Associated order not found.");

            var order = plan.CustomerOrder;

            if (order.ShippedDate != null)
            {
                var deliveredAt = order.DeliveredDate;
                if (deliveredAt != null && DateTime.UtcNow > deliveredAt.Value.AddDays(BnplSystemConstants.FreeTrialPeriodDays))
                    throw new Exception($"Cancellation not allowed after {BnplSystemConstants.FreeTrialPeriodDays} days of delivery.");
            }

            installment.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Cancelled;
            installment.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(id, installment);
            _logger.LogInformation("Installment {Id} cancelled successfully.", id);

            return installment;
        }

        public async Task<List<BNPL_Installment>> ApplyPaymentToInstallmentAsync(int installmentId, decimal paymentAmount)
        {
            if (paymentAmount <= 0)
                throw new Exception("Payment amount must be greater than zero.");

            var installment = await _repository.GetByIdAsync(installmentId)
                ?? throw new Exception("Installment not found.");

            var planId = installment.Bnpl_PlanID;
            var allInstallments = (await _repository.GetAllByPlanIdAsync(planId))
                .OrderBy(i => i.InstallmentNo)
                .ToList();

            decimal remaining = paymentAmount;
            var updated = new List<BNPL_Installment>();

            _logger.LogInformation("Applying total payment of {Amount} to plan {PlanId}, starting at installment {InstNo}.",
                paymentAmount, planId, installment.InstallmentNo);

            foreach (var inst in allInstallments.Where(i => i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_OnTime &&
                                                            i.Bnpl_Installment_Status != BNPL_Installment_StatusEnum.Paid_Late))
            {
                if (remaining <= 0)
                    break;

                remaining = ApplyPaymentToSingleInstallment(inst, remaining);
                inst.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(inst.InstallmentID, inst);
                updated.Add(inst);

                _logger.LogInformation("Installment {No} processed, remaining balance {Rem}.", inst.InstallmentNo, remaining);
            }

            if (remaining > 0)
            {
                // Still leftover after all installments
                var last = allInstallments.Last();
                last.OverPaymentCarried += remaining;
                await _repository.UpdateAsync(last.InstallmentID, last);
                updated.Add(last);
                _logger.LogInformation("Final overpayment {Rem} carried forward from installment {Id}.", remaining, last.InstallmentID);
            }

            return updated;
        }

        //Helper method
        private decimal ApplyPaymentToSingleInstallment(BNPL_Installment installment, decimal paymentAmount)
        {
            decimal remaining = paymentAmount;
            var now = DateTime.UtcNow;
            bool withinGrace = now <= installment.Installment_DueDate.AddDays(BnplSystemConstants.FreeTrialPeriodDays);

            // Arrears
            if (installment.ArrearsCarried > 0 && remaining > 0)
            {
                decimal pay = Math.Min(installment.ArrearsCarried, remaining);
                installment.ArrearsCarried -= pay;
                remaining -= pay;
            }

            // Late Interest
            if (installment.LateInterest > 0 && remaining > 0)
            {
                decimal pay = Math.Min((decimal)installment.LateInterest, remaining);
                installment.LateInterest -= (double)pay;
                remaining -= pay;
            }

            // Base Amount
            var baseRemaining = installment.Installment_BaseAmount - installment.AmountPaid;
            if (baseRemaining > 0 && remaining > 0)
            {
                decimal pay = Math.Min(baseRemaining, remaining);
                installment.AmountPaid += pay;
                remaining -= pay;
            }

            // Determine status
            if (installment.AmountPaid >= installment.TotalDueAmount &&
                installment.ArrearsCarried == 0 &&
                installment.LateInterest == 0)
            {
                installment.Bnpl_Installment_Status = withinGrace
                    ? BNPL_Installment_StatusEnum.Paid_OnTime
                    : BNPL_Installment_StatusEnum.Paid_Late;

                installment.LastPaymentDate = now;
            }
            else if (installment.AmountPaid > 0)
            {
                installment.Bnpl_Installment_Status = withinGrace
                    ? BNPL_Installment_StatusEnum.PartiallyPaid_OnTime
                    : BNPL_Installment_StatusEnum.PartiallyPaid_Late;
            }
            else
            {
                installment.Bnpl_Installment_Status = BNPL_Installment_StatusEnum.Pending;
            }

            return remaining;
        }
    }
}