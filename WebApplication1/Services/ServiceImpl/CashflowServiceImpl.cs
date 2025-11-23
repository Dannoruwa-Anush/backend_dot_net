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

        //Custom Query Operations
        public async Task<PaginationResultDto<Cashflow>> GetAllWithPaginationAsync(int pageNumber, int pageSize, int? cashflowStatusId = null, string? searchKey = null)
        {
            return await _repository.GetAllWithPaginationAsync(pageNumber, pageSize, cashflowStatusId, searchKey);
        }

        public async Task<decimal> SumCashflowsByOrderAsync(int orderId) =>
            await _repository.SumCashflowsByOrderAsync(orderId);

        //Shared Internal Operations Used by Multiple Repositories
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

            var duplicate = await _repository.ExistsByCashflowRefAsync(newCashflow.CashflowRef);
            if (duplicate)
                throw new Exception($"Cash flow with red '{newCashflow.CashflowRef}' already exists.");

            await _repository.AddAsync(newCashflow);
            _logger.LogInformation("Generated Cashflow record: {CashflowRef}", newCashflow.CashflowRef);
            return newCashflow;
        }
    }
}