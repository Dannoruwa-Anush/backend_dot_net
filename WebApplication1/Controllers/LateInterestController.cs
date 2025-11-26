using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService.Helper;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LateInterestController : ControllerBase
    {
        //Note : LateInterest - Create snapshot (transfer current installment to arreas)
        
        private readonly ILateInterestService _service;

        // Constructor
        public LateInterestController(ILateInterestService service)
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
                await _service.ApplyLateInterestForAllPlansAsync();
                return Ok(new ApiResponseDto<string>(
                    200,
                    "Late interest applied successfully to all overdue installments."
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