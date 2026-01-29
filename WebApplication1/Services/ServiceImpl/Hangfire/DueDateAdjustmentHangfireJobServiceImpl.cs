using Hangfire;
using WebApplication1.Services.IService.Hangfire;
using WebApplication1.Services.IService.Helper;

namespace WebApplication1.Services.ServiceImpl.Hangfire
{
    public class DueDateAdjustmentHangfireJobServiceImpl : IDueDateAdjustmentHangfireJobService
    {
        private readonly IDueDateAdjustmentService _service;
        //logger: for auditing
        private readonly ILogger<DueDateAdjustmentHangfireJobServiceImpl> _logger;

        // Constructor
        public DueDateAdjustmentHangfireJobServiceImpl(IDueDateAdjustmentService service, ILogger<DueDateAdjustmentHangfireJobServiceImpl> logger)
        {
            _service = service;
            _logger = logger;
        }

        //Hangfire job operations
        [DisableConcurrentExecution(600)] // Prevent overlapping executions
        public async Task RunAsync()
        {
            _logger.LogInformation("BNPL Due Date Adjustment job started");

            await _service.ProcessDueDateAdjustmentsAsync();

            _logger.LogInformation("BNPL Due Date Adjustment job finished");
        }
    }
}