using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Helpers;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class CashflowServiceImpl : ICashflowService
    {
        private readonly ICashflowRepository _repository;

        //logger: for auditing
        private readonly ILogger<CashflowServiceImpl> _logger;

        // Constructor
        public CashflowServiceImpl(ICashflowRepository repository, ILogger<CashflowServiceImpl> logger)
        {
            // Dependency injection
            _repository = repository;
            _logger = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Cashflow>> GetAllCashflowsAsync() =>
            await _repository.GetAllAsync();

        public async Task<Cashflow?> GetCashflowByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        public async Task<Cashflow> BuildCashflowAddRequestAsync(PaymentRequestDto paymentRequest, CashflowTypeEnum cashflowType)
        {
            if (paymentRequest == null)
                throw new ArgumentNullException(nameof(paymentRequest));

            // Determine status (default: Paid)
            var status = CashflowStatusEnum.Paid;

            var now = TimeZoneHelper.ToSriLankaTime(DateTime.UtcNow);
            
            // Build reference
            var cashflowRef = $"CF-{paymentRequest.OrderId}-{status}-{cashflowType}-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";

            var newCashflow = new Cashflow
            {
                OrderID = paymentRequest.OrderId,
                AmountPaid = paymentRequest.PaymentAmount,
                CashflowDate = now,
                CashflowStatus = status,
                CashflowRef = cashflowRef
            };
            
            return newCashflow;
        }

        public async Task<Cashflow?> UpdateCashflowAsync(int id, Cashflow updatedCashflow)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                throw new InvalidOperationException("Cashflow record not found.");

            var order = existing.CustomerOrder;
            if (order == null)
                throw new InvalidOperationException("Associated order not found for cashflow.");

            // Validate if cancellation is allowed 
            var now = DateTime.UtcNow;
            bool canCancel = false;

            if (order.OrderStatus == OrderStatusEnum.Pending)
            {
                canCancel = true; // Always allowed before shipping
            }
            else if (order.OrderStatus == OrderStatusEnum.Shipped)
            {
                canCancel = false; // Not allowed after shipped
            }
            else if (order.OrderStatus == OrderStatusEnum.Delivered)
            {
                if (order.DeliveredDate.HasValue)
                {
                    var daysSinceDelivery = (now - order.DeliveredDate.Value).TotalDays;
                    canCancel = daysSinceDelivery <= 14; // Allowed only within 14 days
                }
            }

            if (!canCancel)
                throw new InvalidOperationException("Refund not allowed. Orders can only be cancelled before shipping or within 14 days after delivery.");

            // Perform the refund update
            existing.CashflowStatus = CashflowStatusEnum.Refunded;
            existing.RefundDate = now;
            existing.UpdatedAt = now;

            await _repository.UpdateAsync(id, existing);

            _logger.LogInformation("Cashflow refunded: ID={Id}, OrderID={OrderId}, RefundDate={RefundDate}", existing.CashflowID, existing.OrderID, existing.RefundDate);

            return existing;
        }

        //Custom Query Operations
        public async Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, cashflowStatusId, searchKey);
        }

        public async Task<decimal> SumCashflowsByOrderAsync(int orderId) =>
            await _repository.SumCashflowsByOrderAsync(orderId);
    }
}