using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Settings;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //api/BNPL_PlanType
    public class BNPL_PlanTypeController : ControllerBase
    {
        private readonly IBNPL_PlanTypeService _service;

        private readonly IMapper _mapper;

        // Constructor
        public BNPL_PlanTypeController(IBNPL_PlanTypeService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetAll()
        {
            var bNPL_PlanTypes = await _service.GetAllBNPL_PlanTypesAsync();
            if (bNPL_PlanTypes == null || !bNPL_PlanTypes.Any())
                return NotFound(new ApiResponseDto<string>(404, "BNPL Plan Types not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<BNPL_PlanTypeResponseDto>>(bNPL_PlanTypes);
            var response = new ApiResponseDto<IEnumerable<BNPL_PlanTypeResponseDto>>(
                200,
                "All BNPL Plan Types retrieved successfully",
                responseDtos
            );

            return Ok(response);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetById(int id)
        {
            var bNPL_PlanType = await _service.GetBNPL_PlanTypeByIdAsync(id);
            if (bNPL_PlanType == null)
                return NotFound(new ApiResponseDto<string>(404, "BNPL plan not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<BNPL_PlanTypeResponseDto>(bNPL_PlanType);
            var response = new ApiResponseDto<BNPL_PlanTypeResponseDto>(200, "BNPL plan type retrieved successfully", responseDtos);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> Create([FromBody] BNPL_PlanTypeRequestDto bNPL_PlanTypeCreateDto)
        {
            try
            {
                // RequestDto -> Model
                var bNPL_PlanType = _mapper.Map<BNPL_PlanType>(bNPL_PlanTypeCreateDto);
                var created = await _service.AddBNPL_PlanTypeWithSaveAsync(bNPL_PlanType);

                // Model -> ResponseDto
                var responseDtos = _mapper.Map<BNPL_PlanTypeResponseDto>(created);
                var response = new ApiResponseDto<BNPL_PlanTypeResponseDto>(201, "BNPL Plan Type created successfully", responseDtos);

                return CreatedAtAction(nameof(GetById), new { id = responseDtos.Bnpl_PlanTypeID }, response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "BNPL Plan Type not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "BNPL Plan Type with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> Update(int id, [FromBody] BNPL_PlanTypeRequestDto bNPL_PlanTypeUpdateDto)
        {
            try
            {
                // RequestDto -> Model
                var bNPL_PlanType = _mapper.Map<BNPL_PlanType>(bNPL_PlanTypeUpdateDto);
                var updated = await _service.UpdateBNPL_PlanTypeWithSaveAsync(id, bNPL_PlanType);

                // Model -> ResponseDto
                var responseDtos = _mapper.Map<BNPL_PlanTypeResponseDto>(updated);
                var response = new ApiResponseDto<BNPL_PlanTypeResponseDto>(200, "BNPL Plan Type updated successfully", responseDtos);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "BNPL  Plan Type not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "BNPL Plan Type with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]  // JWT is required
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteBNPL_PlanTypeWithSaveAsync(id);

                var response = new ApiResponseDto<string>(204, "BNPL Plan Type deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "BNPL Plan Type not found"));

                // Generic error
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //Custom Query Operations
        [HttpGet("paged")]
        [Authorize(Roles = "Admin, Employee")] // JWT is required
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);

                // Model -> ResponseDto  
                var paginationResponse = _mapper.Map<PaginationResultDto<BNPL_PlanTypeResponseDto>>(pageResultDto);

                var response = new ApiResponseDto<PaginationResultDto<BNPL_PlanTypeResponseDto>>(
                    200,
                    "BNPL Plan Types retrieved successfully with pagination",
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