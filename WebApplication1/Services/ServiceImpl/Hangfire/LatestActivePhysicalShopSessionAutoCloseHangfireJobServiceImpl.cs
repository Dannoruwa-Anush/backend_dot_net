using Hangfire;
using WebApplication1.Services.IService;
using WebApplication1.Services.IService.Hangfire;

namespace WebApplication1.Services.ServiceImpl.Hangfire
{
    public class LatestActivePhysicalShopSessionAutoCloseHangfireJobServiceImpl : ILatestActivePhysicalShopSessionAutoCloseHangfireJobService
    {
        private readonly IPhysicalShopSessionService _service;

        //logger: for auditing
        private readonly ILogger<LatestActivePhysicalShopSessionAutoCloseHangfireJobServiceImpl> _logger;

        // Constructor
        public LatestActivePhysicalShopSessionAutoCloseHangfireJobServiceImpl(IPhysicalShopSessionService service, ILogger<LatestActivePhysicalShopSessionAutoCloseHangfireJobServiceImpl> logger)
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
                await _service.AutoCloseLatestActiveSessionAsync();

                _logger.LogInformation(
                    "Latest active physical shop auto-closing job completed successfully at {Time}",
                    DateTime.UtcNow
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Latest active physical shop auto-closing job failed at {Time}", DateTime.UtcNow);

                throw; // important: let Hangfire handle retries
            }
        }
    }
}