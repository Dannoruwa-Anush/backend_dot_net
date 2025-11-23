using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService.Helper;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        // Constructor
        public PaymentController(IPaymentService service)
        {
            // Dependency injection
            _service = service;
        }

        [HttpPost("process-payment")]
        public async Task<IActionResult> ProcessPaymentAsync(PaymentRequestDto paymentRequest)
        {
            try
            {
                await _service.ProcessPaymentAsync(paymentRequest);
                return Ok(new ApiResponseDto<string>(
                    200,
                    "Payment processed and stored."
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