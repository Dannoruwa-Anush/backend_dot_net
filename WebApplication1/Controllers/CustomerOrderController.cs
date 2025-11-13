using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.RequestDto.Custom;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // JWT is not required
    public class CustomerOrderController : ControllerBase
    {
        private readonly ICustomerOrderService _service;

        private readonly IMapper _mapper;

        // Constructor
        public CustomerOrderController(ICustomerOrderService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
         [HttpGet]
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
        public async Task<IActionResult> Create([FromBody] CustomerOrderRequestDto customerCreateDto)
        {
            // RequestDto -> Model
            var customerOrder = _mapper.Map<CustomerOrder>(customerCreateDto);
            var created = await _service.AddCustomerOrderAsync(customerOrder);

            // Model -> ResponseDto
            var responseDto = _mapper.Map<CustomerOrderResponseDto>(created);
            var response = new ApiResponseDto<CustomerOrderResponseDto>(201, "Customer order created successfully", responseDto);

            return CreatedAtAction(nameof(GetById), new { id = responseDto.OrderID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerOrderUpdateDto orderUpdateDto)
        {
            try
            {
                var updatedOrder = await _service.UpdateCustomerOrderAsync(id, orderUpdateDto);

                var responseDto = _mapper.Map<CustomerOrderResponseDto>(updatedOrder);
                var response = new ApiResponseDto<CustomerOrderResponseDto>(
                    200,
                    "Customer order updated successfully",
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
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, int? paymentStatusId = null, int? orderStatusId = null, string? searchKey = null)
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
    }
}