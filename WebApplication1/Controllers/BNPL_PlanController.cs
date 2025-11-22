using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BNPL_PlanController : ControllerBase
    {
        //Note: All bnpl plan creation and updating process will be handled by PaymentController/ CustomerOrderController : cancel
        //Note: This controller is only for list down all bnpl plans

        private readonly IBNPL_PlanService _service;

        private readonly IMapper _mapper;

        // Constructor
        public BNPL_PlanController(IBNPL_PlanService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetAll()
        {
            var bnpl_Plans = await _service.GetAllBNPL_PlansAsync();
            if (bnpl_Plans == null || !bnpl_Plans.Any())
                return NotFound(new ApiResponseDto<string>(404, "Bnpl Plans not found"));
            
            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<BNPL_PlanResponseDto>>(bnpl_Plans);
            var response = new ApiResponseDto<IEnumerable<BNPL_PlanResponseDto>>(
                200,
                "All Bnpl Plans retrieved successfully",
                responseDtos
            );
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetById(int id)
        {
            var bnpl_Plan = await _service.GetBNPL_PlanByIdAsync(id);
            if (bnpl_Plan == null)
                return NotFound(new ApiResponseDto<string>(404, "BNPL plan not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<BNPL_PlanResponseDto>(bnpl_Plan);
            var response = new ApiResponseDto<BNPL_PlanResponseDto>(200, "BNPL Plans retrieved successfully", responseDtos);

            return Ok(response);
        }

        //Custom Query Operations
        [HttpGet("paged")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, int? planStatusId = null,  string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, planStatusId, searchKey);
                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<BNPL_PlanResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<BNPL_PlanResponseDto>>(
                    200,
                    "Bnpl Plans retrieved successfully with pagination",
                    paginationResponse
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //calculator
        [HttpPost("calculateInstallment")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> CalculateInstallment([FromBody] BNPLInstallmentCalculatorRequestDto request)
        {
            try
            {
                var result = await _service.CalculateBNPL_PlanAmountPerInstallmentAsync(request);

                var response = new ApiResponseDto<BNPLInstallmentCalculatorResponseDto>(
                    200,
                    "BNPL installment calculation successful",
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
    }
}