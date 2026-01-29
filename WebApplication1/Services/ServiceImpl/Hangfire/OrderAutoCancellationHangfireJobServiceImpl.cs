using Hangfire;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Hangfire;

namespace WebApplication1.Services.ServiceImpl.Hangfire
{
    public class OrderAutoCancellationHangfireJobServiceImpl : IOrderAutoCancellationHangfireJobService
    {
        private readonly ICustomerOrderService _service;

        //logger: for auditing
        private readonly ILogger<OrderAutoCancellationHangfireJobServiceImpl> _logger;

        // Constructor
        public OrderAutoCancellationHangfireJobServiceImpl(ICustomerOrderService service, ILogger<OrderAutoCancellationHangfireJobServiceImpl> logger)
        {
            // Dependency injection
            _service = service;
            _logger = logger;
        }

        //Hangfire job operations
        // Prevent overlapping executions
        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        // Retry only for transient failures
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail, DelaysInSeconds = new[] { 30, 60, 120 })]
        public async Task RunAsync()
        {
            _logger.LogInformation("Order auto-cancellation job started at {Time}", DateTime.UtcNow);

            try
            {
                await _service.AutoCancelExpiredOnlineOrdersAsync();

                _logger.LogInformation(
                    "Order auto-cancellation job completed successfully at {Time}",
                    DateTime.UtcNow
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order auto-cancellation job failed at {Time}", DateTime.UtcNow);

                throw; // important: let Hangfire handle retries
            }
        }
    }
}