using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService.Helper;
using WebApplication1.Utils.Settings;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _service;

        private readonly IMapper _mapper;

        // Constructor
        public PaymentController(IPaymentService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        [HttpPost("process-payment")]
        [Authorize(Policy = AuthorizationPolicies.CashierOrCustomer)]  // JWT is required
        public async Task<IActionResult> ProcessFullPaymentAsync(PaymentRequestDto paymentRequest)
        {
            try
            {
                await _service.ProcessPaymentAsync(paymentRequest);
                return Ok(new ApiResponseDto<string>(
                    200,
                    "Payment processed and stored."
                ));
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Invoice not found"));

                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }
    }
}