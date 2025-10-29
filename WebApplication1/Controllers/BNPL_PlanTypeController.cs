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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var bNPL_PlanType = await _service.GetBNPL_PlanTypeByIdAsync(id);
            if (bNPL_PlanType == null)
                return NotFound(new ApiResponseDto<string>(404, "BNPL plan not found"));

            var dto = _mapper.Map<BNPL_PlanTypeResponseDto>(bNPL_PlanType);
            var response = new ApiResponseDto<BNPL_PlanTypeResponseDto>(200, "BNPL plan type retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BNPL_PlanTypeRequestDto bNPL_PlanTypeCreateDto)
        {
            var bNPL_PlanType = _mapper.Map<BNPL_PlanType>(bNPL_PlanTypeCreateDto);
            var created = await _service.AddBNPL_PlanTypeAsync(bNPL_PlanType);
            var dto = _mapper.Map<BNPL_PlanTypeResponseDto>(created);

            var response = new ApiResponseDto<BNPL_PlanTypeResponseDto>(201, "BNPL Plan Type created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.Bnpl_PlanTypeID}, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BNPL_PlanTypeRequestDto bNPL_PlanTypeUpdateDto)
        {
            try
            {
                var bNPL_PlanType = _mapper.Map<BNPL_PlanType>(bNPL_PlanTypeUpdateDto);
                var updated = await _service.UpdateBNPL_PlanTypeAsync(id, bNPL_PlanType);
                var dto = _mapper.Map<BNPL_PlanTypeResponseDto>(updated);

                var response = new ApiResponseDto<BNPL_PlanTypeResponseDto>(200, "BNPL Plan Type updated successfully", dto);
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteBNPL_PlanTypeAsync(id);

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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
            {
                // Call service to get paginated data
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber.Value, pageSize.Value);

                var dtos = _mapper.Map<IEnumerable<BNPL_PlanTypeResponseDto>>(pageResultDto.Items);

                var paginationResult = new PaginationResultDto<BNPL_PlanTypeResponseDto>
                {
                    Items      = dtos,
                    TotalCount = pageResultDto.TotalCount,
                    PageNumber = pageResultDto.PageNumber,
                    PageSize   = pageResultDto.PageSize
                };

                var response = new ApiResponseDto<PaginationResultDto<BNPL_PlanTypeResponseDto>>(
                    200,
                    "BNPL Plan Types retrieved successfully with pagination",
                    paginationResult
                );

                return Ok(response);
            }
            else
            {
                // Return all data without pagination
                var bNPL_PlanTypes = await _service.GetAllBNPL_PlanTypesAsync();
                var dtos = _mapper.Map<IEnumerable<BrandResponseDto>>(bNPL_PlanTypes);

                var response = new ApiResponseDto<IEnumerable<BrandResponseDto>>(
                    200,
                    "All BNPL Plan Types retrieved successfully",
                    dtos
                );

                return Ok(response);
            }
        }
    }
}