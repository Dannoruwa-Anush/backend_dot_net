using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.RequestDto.StatusChange;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerOrderController : ControllerBase
    {
        private readonly ICustomerOrderService _service;
        private readonly IDocumentGenerationService _documentGenerationService;
        private readonly IWebHostEnvironment _env;

        private readonly IMapper _mapper;

        // Constructor
        public CustomerOrderController(ICustomerOrderService service, IDocumentGenerationService documentGenerationService, IWebHostEnvironment env, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _documentGenerationService = documentGenerationService;
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
                return NotFound(new ApiResponseDto<string>(404, "Customer order not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<CustomerOrderResponseDto>(customerOrder);
            var response = new ApiResponseDto<CustomerOrderResponseDto>(200, "Customer order retrieved successfully", responseDto);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Employee, Customer")] // JWT is required
        public async Task<IActionResult> Create([FromBody] CustomerOrderRequestDto customerOrderCreateDto)
        {
            try
            {
                var created = await _service.CreateCustomerOrderWithTransactionAsync(customerOrderCreateDto);

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
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] CustomerOrderStatusChangeRequestDto request)
        {
            try
            {
                var updatedOrder = await _service.ModifyCustomerOrderStatusWithTransactionAsync(id, request);

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
     
        [HttpGet("bnpl")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetActiveBnpOrderById([FromQuery] int id, [FromQuery] int? customerId)
        {
            var customerOrder = await _service.GetCustomerOrderWithActiveBnplByIdAsync(id, customerId);
            if (customerOrder == null)
                return NotFound(new ApiResponseDto<string>(404, "Customer order not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<CustomerOrderResponseDto>(customerOrder);
            var response = new ApiResponseDto<CustomerOrderResponseDto>(200, "Customer order retrieved successfully", responseDto);

            return Ok(response);
        }

        //CRUD operations
        [HttpGet("bnpl/customer/{customerId}")]
        [Authorize(Roles = "Admin, Employee, Customer")] // JWT is required
        public async Task<IActionResult> GetAllActiveBnplCustomerOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _service.GetAllActiveBnplCustomerOrdersByCustomerIdAsync(customerId);
            if (orders == null || !orders.Any())
                return NotFound(new ApiResponseDto<string>(404, "Active bnpl order not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<IEnumerable<CustomerOrderResponseDto>>(orders);
            var response = new ApiResponseDto<IEnumerable<CustomerOrderResponseDto>>(
                200,
                "All customer active bnpl orders retrieved successfully",
                responseDto
            );
            return Ok(response);
        }
    }
}