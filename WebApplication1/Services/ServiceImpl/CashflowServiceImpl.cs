using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Repositories.IRepository;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class CashflowServiceImpl : ICashflowService
    {
        private readonly ICashflowRepository _repository;
        private readonly ICustomerOrderRepository _customerOrderRepository;

        //logger: for auditing
        private readonly ILogger<CashflowServiceImpl> _logger;

        // Constructor
        public CashflowServiceImpl(ICashflowRepository repository, ICustomerOrderRepository customerOrderRepository, ILogger<CashflowServiceImpl> logger)
        {
            // Dependency injection
            _repository              = repository;
            _customerOrderRepository = customerOrderRepository;
            _logger                  = logger;
        }

        //CRUD operations
        public async Task<IEnumerable<Cashflow>> GetAllCashflowsAsync() =>
            await _repository.GetAllAsync();

        public async Task<Cashflow?> GetCashflowByIdAsync(int id) =>
            await _repository.GetByIdAsync(id);

        /*
        public async Task<Cashflow> AddCashflowAsync(CashflowRequestDto request)
        {
            var order = await _customerOrderRepository.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new Exception($"Customer order with ID {request.OrderId} not found.");

            if (request.Type == CashflowTypeEnum.BnplInstallmentPayment && request.InstallmentNo is null)
                throw new ArgumentException("Installment number is required for BNPL installment payments.");

            var cashflow = new Cashflow
            {
                OrderID = order.OrderID,
                CustomerOrder = order,
                AmountPaid = request.AmountPaid,
                CashflowDate = DateTime.UtcNow,
                CashflowStatus = CashflowStatusEnum.Paid,
                CreatedAt = DateTime.UtcNow
            };

            switch (request.Type)
            {
                case CashflowTypeEnum.FullPayment:
                    cashflow.CashflowRef = $"{order.OrderID}_full_payment";
                    break;

                case CashflowTypeEnum.BnplInitialPayment:
                    cashflow.CashflowRef = $"{order.OrderID}_bnpl_initial_payment";
                    break;    

                case CashflowTypeEnum.BnplInstallmentPayment:
                    var installmentType = request.IsFullInstallmentPayment ? "full_payment" : "partial_payment";
                    cashflow.CashflowRef = $"{order.OrderID}_bnpl_installment_{request.InstallmentNo}_{installmentType}";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(request.Type), "Unsupported cashflow type");
            }

            await _repository.AddAsync(cashflow);
            _logger.LogInformation("Cashflow created for OrderID={OrderID}, Type={Type}, Amount={Amount}, Ref={Ref}", order.OrderID, request.Type, request.AmountPaid, cashflow.CashflowRef);

            return cashflow;
        }
        */
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
    }
}