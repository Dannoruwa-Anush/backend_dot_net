using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
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

        //Custom Query Operations
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var bnpl_snapshot = await _service.GetLatestSnapshotWithOrderDetailsAsync(orderId);
            if (bnpl_snapshot == null)
                return NotFound(new ApiResponseDto<string>(404, "Bnpl latest snapshot not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<BNPL_PlanSettlementSummaryResponseDto>(bnpl_snapshot);
            var response = new ApiResponseDto<BNPL_PlanSettlementSummaryResponseDto>(200, "Bnpl latest snapshot retrieved successfully", responseDtos);

            return Ok(response);
        }

        //Installment Payment Simulator
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