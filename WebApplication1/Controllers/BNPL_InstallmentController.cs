using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cashflow = await _service.GetBNPL_InstallmentByIdAsync(id);
            if (cashflow == null)
                return NotFound(new ApiResponseDto<string>(404, "Bnpl installment not found"));

            var dto = _mapper.Map<BNPL_InstallmentResponseDto>(cashflow);
            var response = new ApiResponseDto<BNPL_InstallmentResponseDto>(200, "Bnpl installment retrieved successfully", dto);

            return Ok(response);
        }

        //Custom Query Operations
        [HttpGet]
        public async
        Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? bnpl_Installment_StatusId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var result = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, bnpl_Installment_StatusId, searchKey);

                var response = new ApiResponseDto<PaginationResultDto<BNPL_Installment>>(
                    200,
                    "Bnpl installment records retrieved successfully",
                    result
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //For testing : Manual trigger
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