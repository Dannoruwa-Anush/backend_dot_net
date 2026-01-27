using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.RequestDto.UserProfileUpdate;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Settings;

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
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]  // JWT is required
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
        [Authorize(Policy = AuthorizationPolicies.AdminOrManager)]  // JWT is required
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _service.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound(new ApiResponseDto<string>(404, "Employee not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<EmployeeResponseDto>(employee);
            var response = new ApiResponseDto<EmployeeResponseDto>(200, "Employee retrieved successfully", responseDtos);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]  // JWT is required
        public async Task<IActionResult> Create([FromBody] EmployeeRequestDto employeeCreateDto)
        {
            try
            {
                // RequestDto -> Model
                var employee = _mapper.Map<Employee>(employeeCreateDto);
                var created = await _service.CreateEmployeeWithTransactionAsync(employee);

                // Model -> ResponseDto
                var responseDto = _mapper.Map<EmployeeResponseDto>(created);
                var response = new ApiResponseDto<EmployeeResponseDto>(201, "Employee created successfully", responseDto);

                return CreatedAtAction(nameof(GetById), new { id = responseDto.EmployeeID }, response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Employee with the given email already exists"));

                return StatusCode(500, new ApiResponseDto<string>(
                    500,
                    "An internal server error occurred. Please try again later."
                ));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]  // JWT is required
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeUpdateRequestDto employeeUpdateDto)
        {
            try
            {
                // RequestDto -> Model
                var employee = _mapper.Map<Employee>(employeeUpdateDto);
                var updated = await _service.UpdateEmployeeWithSaveAsync(id, employee);

                var responseDto = _mapper.Map<EmployeeResponseDto>(updated);
                var response = new ApiResponseDto<EmployeeResponseDto>(200, "Employee updated successfully", responseDto);
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

        [HttpPut("profile/{id}")]
        [Authorize(Policy = AuthorizationPolicies.AllEmployeesOnly)]  // JWT is required
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] EmployeeProfileUpdateRequestDto employeeProfileUpdateDto)
        {
            try
            {
                // RequestDto -> Model
                var employee = _mapper.Map<Employee>(employeeProfileUpdateDto);
                var updated = await _service.UpdateEmployeeProfileWithSaveAsync(id, employee);

                var responseDto = _mapper.Map<EmployeeResponseDto>(updated);
                var response = new ApiResponseDto<EmployeeResponseDto>(200, "Employee profile updated successfully", responseDto);
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
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]  // JWT is required
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? positionId = null, [FromQuery] string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, positionId, searchKey);
                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<EmployeeResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<EmployeeResponseDto>>(
                    200,
                    "Employee records retrieved successfully",
                    paginationResponse
                );

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = AuthorizationPolicies.AllEmployeesOnly)]  // JWT is required
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var employee = await _service.GetEmployeeByUserIdAsync(userId);
            if (employee == null)
                return NotFound(new ApiResponseDto<string>(404, "Employee not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<EmployeeResponseDto>(employee);
            var response = new ApiResponseDto<EmployeeResponseDto>(200, "Employee retrieved successfully", responseDtos);

            return Ok(response);
        }
    }
}