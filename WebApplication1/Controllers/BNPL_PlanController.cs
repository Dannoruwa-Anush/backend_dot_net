using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.BnplCal;
using WebApplication1.DTOs.ResponseDto.Common;
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






        //Custom Query Operations
        [HttpPost("calculateInstallment")]
        public async Task<IActionResult> Calculate([FromBody] BNPLInstallmentCalculatorRequestDto request)
        {
            try
            {
                var result = await _service.CalculateAmountPerInstallmentAsync(request);

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