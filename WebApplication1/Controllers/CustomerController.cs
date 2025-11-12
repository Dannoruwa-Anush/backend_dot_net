using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //api/customer 
    [AllowAnonymous] // JWT is not required for this controller
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _service;

        private readonly IMapper _mapper;

        // Constructor
        public CustomerController(ICustomerService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet]
        [AllowAnonymous] // JWT is not required 
        public async Task<IActionResult> GetAll()
        {
            var customers = await _service.GetAllCustomersAsync();
            if (customers == null || !customers.Any())
                return NotFound(new ApiResponseDto<string>(404, "Customers not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<CustomerResponseDto>>(customers);
            var response = new ApiResponseDto<IEnumerable<CustomerResponseDto>>(
                200,
                "All Customers retrieved successfully",
                responseDtos
            );
            return Ok(response);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _service.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new ApiResponseDto<string>(404, "Customer not found"));

            var dto = _mapper.Map<CustomerResponseDto>(customer);
            var response = new ApiResponseDto<CustomerResponseDto>(200, "Customer retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerRequestDto customerCreateDto)
        {
            var customer = _mapper.Map<Customer>(customerCreateDto);
            var created = await _service.AddCustomerAsync(customer);
            var dto = _mapper.Map<CustomerResponseDto>(created);

            var response = new ApiResponseDto<CustomerResponseDto>(201, "Customer created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.CustomerID}, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerRequestDto customerUpdateDto)
        {
            try
            {
                var customer = _mapper.Map<Customer>(customerUpdateDto);
                var updated = await _service.UpdateCustomerAsync(id, customer);
                var dto = _mapper.Map<CustomerResponseDto>(updated);

                var response = new ApiResponseDto<CustomerResponseDto>(200, "Customer updated successfully", dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Customer not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Customer with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteCustomerAsync(id);

                var response = new ApiResponseDto<string>(204, "Customer deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Customer not found"));

                // Generic error
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //Custom Query Operations
        [HttpGet("paged")]
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchKey = null)
        {
            try
            {
                var result = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);

                var response = new ApiResponseDto<PaginationResultDto<Customer>>(
                    200,
                    "Customer records retrieved successfully",
                    result
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