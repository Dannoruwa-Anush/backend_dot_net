using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class BNPL_InstallmentController : ControllerBase
    {
        //Note: All installment payment/ installment payment refund will be handled by PaymentController
        //Note: This controller is only for list down all installments

        private readonly IBNPL_InstallmentService _service;
        private readonly IMapper _mapper;

        // Constructor
        public BNPL_InstallmentController(IBNPL_InstallmentService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var bnpl_installments = await _service.GetAllBNPL_InstallmentsAsync();
            if (bnpl_installments == null || !bnpl_installments.Any())
                return NotFound(new ApiResponseDto<string>(404, "Bnpl installments not found"));
            
            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<BNPL_InstallmentResponseDto>>(bnpl_installments);
            var response = new ApiResponseDto<IEnumerable<BNPL_InstallmentResponseDto>>(
                200,
                "All Bnpl installments retrieved successfully",
                responseDtos
            );
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bnpl_installment = await _service.GetBNPL_InstallmentByIdAsync(id);
            if (bnpl_installment == null)
                return NotFound(new ApiResponseDto<string>(404, "Bnpl installment not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<BNPL_InstallmentResponseDto>(bnpl_installment);
            var response = new ApiResponseDto<BNPL_InstallmentResponseDto>(200, "Bnpl installment retrieved successfully", responseDtos);

            return Ok(response);
        }

        //Custom Query Operations
        [HttpGet("paged")]
        public async
        Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? bnpl_Installment_StatusId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);
                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<BNPL_InstallmentResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<BNPL_InstallmentResponseDto>>(
                    200,
                    "Bnpl installment records retrieved successfully",
                    paginationResponse
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //Custom Query Operations
        [HttpGet("paged/order")]
        public async
        Task<IActionResult> GetAllWithPaginationByOrderIdAsync([FromQuery] int orderId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? bnpl_Installment_StatusId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationByOrderIdAsync(orderId, pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);
                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<BNPL_InstallmentResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<BNPL_InstallmentResponseDto>>(
                    200,
                    "Bnpl installment records of the order retrieved successfully",
                    paginationResponse
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //Installment Paymenr Simulator
        /*
        [HttpPost("bnpl-installmant-payment-simulate")]
        public async Task<IActionResult> SimulateBnplInstallmentPayment([FromBody] BnplInstallmentPaymentSimulationRequestDto request)
        {
            try
            {
                var result = await _service.SimulateBnplInstallmentPaymentAsync(request);
                var response = new ApiResponseDto<object>(
                    200,
                    "Payment simulation successful",
                    result
                );
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<string>(
                    400,
                    ex.Message
                ));
            }
        }
        */
        
        //For testing : Manual trigger (Need to do : automate with bg-process: Hangfire)
        [HttpPost("apply-late-interest")]
        public async Task<IActionResult> ApplyLateInterest()
        {
            try
            {
                await _service.ApplyLateInterestAsync();
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