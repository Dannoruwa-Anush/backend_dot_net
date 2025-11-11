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
    [Route("api/[controller]")] //api/employee 
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _service;

        private readonly IMapper _mapper;

        // Constructor
        public EmployeeController(IEmployeeService service, IMapper mapper)
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
            var employees = await _service.GetAllEmployeesAsync();
            if (employees == null || !employees.Any())
                return NotFound(new ApiResponseDto<string>(404, "Employees not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<EmployeeResponseDto>>(employees);
            var response = new ApiResponseDto<IEnumerable<EmployeeResponseDto>>(
                200,
                "All employees retrieved successfully",
                responseDtos
            );
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // JWT is not required 
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _service.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound(new ApiResponseDto<string>(404, "Employee not found"));

            var dto = _mapper.Map<EmployeeResponseDto>(employee);
            var response = new ApiResponseDto<EmployeeResponseDto>(200, "Employee retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // JWT is required
        public async Task<IActionResult> Create([FromBody] EmployeeRequestDto employeeCreateDto)
        {
            var employee = _mapper.Map<Employee>(employeeCreateDto);
            var created = await _service.AddEmployeeAsync(employee);
            var dto = _mapper.Map<EmployeeResponseDto>(created);

            var response = new ApiResponseDto<EmployeeResponseDto>(201, "Employee created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.EmployeeID }, response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // JWT is required
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeRequestDto employeeUpdateDto)
        {
            try
            {
                var employee = _mapper.Map<Employee>(employeeUpdateDto);
                var updated = await _service.UpdateEmployeeAsync(id, employee);
                var dto = _mapper.Map<EmployeeResponseDto>(updated);

                var response = new ApiResponseDto<EmployeeResponseDto>(200, "Employee updated successfully", dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Employee not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Employee with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //Custom Query Operations
        [HttpGet("paged")]
        [AllowAnonymous] // JWT is not required 
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? positionId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var result = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, positionId, searchKey);

                var response = new ApiResponseDto<PaginationResultDto<Employee>>(
                    200,
                    "Employee records retrieved successfully",
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