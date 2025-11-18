using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class BNPL_PlanSettlementSummaryController : ControllerBase
    {

        private readonly IBNPL_PlanSettlementSummaryService _service;
        private readonly IMapper _mapper;

        // Constructor
        public BNPL_PlanSettlementSummaryController(IBNPL_PlanSettlementSummaryService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations

        //Custom Query Operations

        //Installment Paymenr Simulator
        [HttpPost("bnpl-snapshot-payment-simulate")]
        public async Task<IActionResult> SimulateBnplPlanSettlement([FromBody] BnplSnapshotPayingSimulationRequestDto request)
        {
            try
            {
                var result = await _service.SimulateBnplPlanSettlementAsync(request);
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
    }
}