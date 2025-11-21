using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.RequestDto.StatusChange;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerOrderController : ControllerBase
    {
        private readonly ICustomerOrderService _service;
        private readonly IInvoiceService _invoiceService;
        private readonly IWebHostEnvironment _env;

        private readonly IMapper _mapper;

        // Constructor
        public CustomerOrderController(ICustomerOrderService service, IInvoiceService invoiceService, IWebHostEnvironment env, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _invoiceService = invoiceService;
            _env = env;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetAll()
        {
            var orders = await _service.GetAllCustomerOrdersAsync();
            if (orders == null || !orders.Any())
                return NotFound(new ApiResponseDto<string>(404, "Order not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<IEnumerable<CustomerOrderResponseDto>>(orders);
            var response = new ApiResponseDto<IEnumerable<CustomerOrderResponseDto>>(
                200,
                "All customer orders retrieved successfully",
                responseDto
            );
            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Employee, Customer")] // JWT is required
        public async Task<IActionResult> GetById(int id)
        {
            var customerOrder = await _service.GetCustomerOrderByIdAsync(id);
            if (customerOrder == null)
                return NotFound(new ApiResponseDto<string>(404, "Customer not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<CustomerOrderResponseDto>(customerOrder);
            var response = new ApiResponseDto<CustomerOrderResponseDto>(200, "Customer order retrieved successfully", responseDto);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Customer")] // JWT is required
        public async Task<IActionResult> Create([FromBody] CustomerOrderRequestDto customerCreateDto)
        {
            try
            {
                // RequestDto -> Model
                var customerOrder = _mapper.Map<CustomerOrder>(customerCreateDto);
                var created = await _service.CreateCustomerOrderWithTransactionAsync(customerOrder);

                // Model -> ResponseDto
                var responseDto = _mapper.Map<CustomerOrderResponseDto>(created);
                var response = new ApiResponseDto<CustomerOrderResponseDto>(201, "Customer order created successfully", responseDto);

                return CreatedAtAction(nameof(GetById), new { id = responseDto.OrderID }, response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("Insufficient stock", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Insufficient stock. Please try again later."));

                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employee, Customer")] // JWT is required
        public async Task<IActionResult> Update(int id, [FromBody] CustomerOrderStatusChangeRequestDto request)
        {
            try
            {
                var updatedOrder = await _service.ModifyCustomerOrderStatusWithTransactionAsync(request);

                var responseDto = _mapper.Map<CustomerOrderResponseDto>(updatedOrder);
                var response = new ApiResponseDto<CustomerOrderResponseDto>(
                    200,
                    "Customer order status updated successfully",
                    responseDto
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Order not found"));

                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }

        //Custom Query Operations
        [HttpGet("paged")]
        [Authorize(Roles = "Admin, Employee, Customer")] // JWT is required
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, paymentStatusId, orderStatusId, searchKey);
                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<CustomerOrderResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<CustomerOrderResponseDto>>(
                    200,
                    "Customer orders retrieved successfully with pagination",
                    paginationResponse
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpGet("paged/customer")]
        [Authorize(Roles = "Admin, Employee, Customer")] // JWT is required
        public async Task<IActionResult> GetAllByCustomer([FromQuery] int customerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, int? orderStatusId = null, string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllByCustomerWithPaginationAsync(customerId, pageNumber, pageSize, orderStatusId, searchKey);
                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<CustomerOrderResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<CustomerOrderResponseDto>>(
                    200,
                    "Orders of customer retrieved successfully with pagination",
                    paginationResponse
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpGet("invoice/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrderInvoiceByOrderId(int id)
        {
            var customerOrder = await _service.GetCustomerOrderByIdAsync(id);

            if (customerOrder == null)
                return NotFound(new ApiResponseDto<string>(404, "Customer order not found"));

            var orderDto = _mapper.Map<CustomerOrderResponseDto>(customerOrder);

            // Generate invoice PDF using service
            var relativePath = await _invoiceService.GenerateInvoicePdfAsync(orderDto);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new ApiResponseDto<string>(404, "Failed to generate invoice file"));

            // Return file for download
            //var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            //var fileName = Path.GetFileName(fullPath);
            //return File(fileBytes, "application/pdf", fileName);

            var fileUrl = $"{Request.Scheme}://{Request.Host}/{relativePath}";
            return Ok(new ApiResponseDto<string>(200, "Invoice generated successfully", fileUrl));
        }
    }
}