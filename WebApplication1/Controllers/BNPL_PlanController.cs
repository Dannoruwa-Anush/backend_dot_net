using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BNPL_PlanController : ControllerBase
    {
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bnpl_Plan = await _service.GetBNPL_PlanByIdAsync(id);
            if (bnpl_Plan == null)
                return NotFound(new ApiResponseDto<String>(404, "BNPL plan not found"));

            var dto = _mapper.Map<BNPL_PlanResponseDto>(bnpl_Plan);
            var response = new ApiResponseDto<BNPL_PlanResponseDto>(200, "BNPL Plans retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BNPL_PlanRequestDto bNPL_PlanCreateDto)
        {
            var bNPL_Plan = _mapper.Map<BNPL_PLAN>(bNPL_PlanCreateDto);
            var created = await _service.AddBNPL_PlanAsync(bNPL_Plan);
            var dto = _mapper.Map<BNPL_PlanResponseDto>(created);

            var response = new ApiResponseDto<BNPL_PlanResponseDto>(201, "BNPL Plan created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.Bnpl_PlanID}, response);
        }
   
        //Custom Query Operations
        [HttpPost("calculateInstallment")]
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