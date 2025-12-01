using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService.Helper;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class DueDateAdjustmentController : ControllerBase
    {
        //Note : LateInterest - Create snapshot (transfer current installment to arreas)
        
        private readonly IDueDateAdjustmentService _service;

        // Constructor
        public DueDateAdjustmentController(IDueDateAdjustmentService service)
        {
            // Dependency injection
            _service = service;
        }

        //For testing : Manual trigger (Need to do : automate with bg-process: Hangfire)
        [HttpPost("apply-late-interest")]
        public async Task<IActionResult> ApplyLateInterest()
        {
            try
            {
                await _service.ProcessDueDateAdjustmentsAsync();
                return Ok(new ApiResponseDto<string>(
                    200,
                    "Due date adjustment applied successfully to all overdue installments."
                ));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }
    }
}