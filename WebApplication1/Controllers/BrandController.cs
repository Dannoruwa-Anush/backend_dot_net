using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto.Common;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //api/brand 
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _service;
        private readonly IMapper _mapper;

        // Constructor
        public BrandController(IBrandService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var brand = await _service.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound(new ApiResponseDto<string>(404, "Brand not found"));

            var dto = _mapper.Map<BrandResponseDto>(brand);
            var response = new ApiResponseDto<BrandResponseDto>(200, "Brand retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BrandRequestDto brandCreateDto)
        {
            try
            {
                var brand = _mapper.Map<Brand>(brandCreateDto);
                var created = await _service.AddBrandAsync(brand);
                var dto = _mapper.Map<BrandResponseDto>(created);

                var response = new ApiResponseDto<BrandResponseDto>(201, "Brand created successfully", dto);

                return CreatedAtAction(nameof(GetById), new { id = dto.BrandID }, response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Brand name is already exists"));

                // Generic error
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BrandRequestDto brandUpdateDto)
        {
            try
            {
                var brand = _mapper.Map<Brand>(brandUpdateDto);
                var updated = await _service.UpdateBrandAsync(id, brand);
                var dto = _mapper.Map<BrandResponseDto>(updated);

                var response = new ApiResponseDto<BrandResponseDto>(200, "Brand updated successfully", dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Brand not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Brand with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteBrandAsync(id);

                var response = new ApiResponseDto<string>(204, "Brand deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Brand not found"));

                // Generic error
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        //Custom Query Operations
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
            {
                // Call service to get paginated data
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber.Value, pageSize.Value);

                var dtos = _mapper.Map<IEnumerable<BrandResponseDto>>(pageResultDto.Items);

                var paginationResult = new PaginationResultDto<BrandResponseDto>
                {
                    Items = dtos,
                    TotalCount = pageResultDto.TotalCount,
                    PageNumber = pageResultDto.PageNumber,
                    PageSize = pageResultDto.PageSize
                };

                var response = new ApiResponseDto<PaginationResultDto<BrandResponseDto>>(
                    200,
                    "Brands retrieved successfully with pagination",
                    paginationResult
                );

                return Ok(response);
            }
            else
            {
                // Return all data without pagination
                var brands = await _service.GetAllBrandsAsync();
                var dtos = _mapper.Map<IEnumerable<BrandResponseDto>>(brands);

                var response = new ApiResponseDto<IEnumerable<BrandResponseDto>>(
                    200,
                    "All brands retrieved successfully",
                    dtos
                );

                return Ok(response);
            }
        }
    }
}
