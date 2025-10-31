using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CashflowRequestDto cashflowCreateDto)
        {
            try
            {
                var created = await _service.AddCashflowAsync(cashflowCreateDto);
                var dto = _mapper.Map<CashflowResponseDto>(created);

                var response = new ApiResponseDto<CashflowResponseDto>(
                    201,
                    "Cashflow created successfully",
                    dto
                );

                return CreatedAtAction(nameof(GetById), new { id = dto.CashflowID }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponseDto<string>(400, ex.Message));
            }
        }

        //Custom Query Operations
    }
}