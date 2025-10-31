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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customerOrder = await _service.GetCustomerOrderByIdAsync(id);
            if (customerOrder == null)
                return NotFound(new ApiResponseDto<string>(404, "Customer not found"));

            var dto = _mapper.Map<CustomerOrderResponseDto>(customerOrder);
            var response = new ApiResponseDto<CustomerOrderResponseDto>(200, "Customer order retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerOrderRequestDto customerCreateDto)
        {
            var customerOrder = _mapper.Map<CustomerOrder>(customerCreateDto);
            var created = await _service.AddCustomerOrderAsync(customerOrder);
            var dto = _mapper.Map<CustomerOrderResponseDto>(created);

            var response = new ApiResponseDto<CustomerOrderResponseDto>(201, "Customer order created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.OrderID}, response);
        }

        //Custom Query Operations
    }
}