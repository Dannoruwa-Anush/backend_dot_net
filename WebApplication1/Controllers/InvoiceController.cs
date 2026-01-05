using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService;

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
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetById(int id)
        {
            var invoice = await _service.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound(new ApiResponseDto<string>(404, "Invoice not found"));

            var dto = _mapper.Map<InvoiceResponseDto>(invoice);
            var response = new ApiResponseDto<InvoiceResponseDto>(200, "invoice retrieved successfully", dto);

            return Ok(response);
        }

        //Custom Query Operations
        [HttpGet("paged")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? invoiceTypeId = null, [FromQuery] int? invoiceStatusId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, invoiceTypeId, invoiceStatusId, searchKey);

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
    }
}