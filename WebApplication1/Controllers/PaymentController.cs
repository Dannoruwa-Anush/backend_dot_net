using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.Payment;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.DTOs.ResponseDto.Payment.Bnpl;
using WebApplication1.Services.IService.Helper;

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

        [HttpPost("process-full-payment")]
        public async Task<IActionResult> ProcessFullPaymentAsync(PaymentRequestDto paymentRequest)
        {
            try
            {
                await _service.ProcessFullPaymentAsync(paymentRequest);
                return Ok(new ApiResponseDto<string>(
                    200,
                    "Full Payment processed and stored."
                ));
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Order not found"));

                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }

        [HttpPost("process-bnpl-initial-payment")]
        public async Task<IActionResult> ProcessBnplInitialPaymentAsync(BnplInitialPaymentRequestDto paymentRequest)
        {
            try
            {
                var createdBnplPlan = await _service.ProcessInitialBnplPaymentAsync(paymentRequest);
                // Model -> ResponseDto   
                var responseDto = _mapper.Map<BNPL_PlanResponseDto>(createdBnplPlan);
                var response = new ApiResponseDto<BNPL_PlanResponseDto>(200, "Initial Bnpl Payment processed and bnpl plan created.", responseDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Order not found"));

                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }

        [HttpPost("process-bnpl-installment-payment")]
        public async Task<IActionResult> ProcessBnplInstallmentPaymentAsync(PaymentRequestDto paymentRequest)
        {
            try
            {
                var createdBnplPlan = await _service.ProcessBnplInstallmentPaymentAsync(paymentRequest);

                // Model -> ResponseDto   
                var responseDto = _mapper.Map<BnplInstallmentPaymentResultDto>(createdBnplPlan);
                var response = new ApiResponseDto<BnplInstallmentPaymentResultDto>(200, "Initial Bnpl installment processed.", responseDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Order not found"));

                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }
    }
}