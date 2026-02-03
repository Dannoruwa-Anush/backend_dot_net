using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.BnplSnapshotPayingSimulation;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Settings;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        //Note: All inovice will be handled by OrderController/PaymentController
        //Note: This controller is only for list down all invoices
        private readonly IInvoiceService _service;

        private readonly IMapper _mapper;

        // Constructor
        public InvoiceController(IInvoiceService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Employee, Customer")] // JWT is required
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.GetInvoiceWithOrderAsync(id);
            if (invoice == null)
                return NotFound(new ApiResponseDto<string>(404, "Invoice not found"));

            var dto = _mapper.Map<InvoiceResponseDto>(invoice);
            var response = new ApiResponseDto<InvoiceResponseDto>(200, "invoice retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOrManager)]  // JWT is required
        public async Task<IActionResult> UpdateInvoiceStatusToCancel(int id)
        {
            try
            {
                var updated = await _service.UpdateInvoiceWithSaveAsync(id);

                // Model -> ResponseDto
                var responseDto = _mapper.Map<InvoiceResponseDto>(updated);
                var response = new ApiResponseDto<InvoiceResponseDto>(200, "Invoice status updated to cancel successfully", responseDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Invoice not found"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //Custom Query Operations
        [HttpGet("paged")]
        [Authorize(Roles = "Admin, Employee, Customer")] // JWT is required
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? invoiceTypeId = null, [FromQuery] int? invoiceStatusId = null, [FromQuery] int? customerId = null, [FromQuery] int?  orderSourceId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, invoiceTypeId, invoiceStatusId, customerId, orderSourceId, searchKey);

                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<InvoiceResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<InvoiceResponseDto>>(
                    200,
                    "Invoice records retrieved successfully",
                    paginationResponse
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }


        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "Admin, Employee, Customer")]
        public async Task<IActionResult> ExistsUnpaidInvoiceByCustome(int customerId)
        {
            try
            {
                bool hasUnpaidInvoice =
                    await _service.ExistsUnpaidInvoiceByCustomerAsync(customerId);

                var response = new ApiResponseDto<bool>(
                    200,
                    "Unpaid invoice check completed successfully",
                    hasUnpaidInvoice
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500,
                    new ApiResponseDto<string>(
                        500,
                        "An internal server error occurred. Please try again later."
                    ));
            }
        }

        [HttpPost("generate/settlement")]
        [Authorize(Roles = "Admin, Employee")]
        public async Task<IActionResult> GenerateInvoiceForSettlementSimulation([FromBody] BnplSnapshotPayingSimulationRequestDto request)
        {
            try
            {
                var invoice = await _service.GenerateInvoiceForSettlementSimulationAsync(request);

                var dto = _mapper.Map<InvoiceResponseDto>(invoice);

                return Ok(new ApiResponseDto<InvoiceResponseDto>(200, "Invoice generated successfully", dto));
            }
            catch (InvalidOperationException ex)
            {
                // Business rule violations
                return BadRequest(new ApiResponseDto<string>(400, ex.Message, null));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An unexpected error occurred while generating invoice", null));
            }
        }
    }
}