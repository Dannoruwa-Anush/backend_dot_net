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
    public class CashflowController : ControllerBase
    {
        //Note: All payment/ payment refund will be handled by PaymentController
        //Note: This controller is only for list down all cashflows
        private readonly ICashflowService _service;

        private readonly IMapper _mapper;

        // Constructor
        public CashflowController(ICashflowService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var cashflow = await _service.GetCashflowByIdAsync(id);
            if (cashflow == null)
                return NotFound(new ApiResponseDto<string>(404, "Cash flow not found"));

            var dto = _mapper.Map<CashflowResponseDto>(cashflow);
            var response = new ApiResponseDto<CashflowResponseDto>(200, "cashflow retrieved successfully", dto);

            return Ok(response);
        }

        //Custom Query Operations
        [HttpGet]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? cashflowStatusId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var result = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, cashflowStatusId, searchKey);

                var response = new ApiResponseDto<PaginationResultDto<Cashflow>>(
                    200,
                    "Cashflow records retrieved successfully",
                    result
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }
    }
}