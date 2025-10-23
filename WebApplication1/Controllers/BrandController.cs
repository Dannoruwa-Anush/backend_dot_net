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

        public BrandController(IBrandService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var brand = await _service.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound(new ApiResponseDto<string>(404, "Brand not found"));

            var brandDto = _mapper.Map<BrandResponseDto>(brand);
            var response = new ApiResponseDto<BrandResponseDto>(200, "Brand retrieved successfully", brandDto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BrandRequestDto brandCreateDto)
        {
            var brand = _mapper.Map<Brand>(brandCreateDto);
            var created = await _service.AddBrandAsync(brand);
            var brandReadDto = _mapper.Map<BrandResponseDto>(created);

            var response = new ApiResponseDto<BrandResponseDto>(201, "Brand created successfully", brandReadDto);

            return CreatedAtAction(nameof(Get), new { id = brandReadDto.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BrandRequestDto brandUpdateDto)
        {
            try
            {
                var brand = _mapper.Map<Brand>(brandUpdateDto);
                var updated = await _service.UpdateBrandAsync(id, brand);
                var updatedDto = _mapper.Map<BrandResponseDto>(updated);

                var response = new ApiResponseDto<BrandResponseDto>(200, "Brand updated successfully", updatedDto);
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
            {
                // Call service to get paginated data
                var pagedResult = await _service.GetAllWithPaginationAsync(pageNumber.Value, pageSize.Value);

                var brandDtos = _mapper.Map<IEnumerable<BrandResponseDto>>(pagedResult.Items);

                var pagedResultDto = new PaginationResultDto<BrandResponseDto>
                {
                    Items = brandDtos,
                    TotalCount = pagedResult.TotalCount,
                    PageNumber = pagedResult.PageNumber,
                    PageSize = pagedResult.PageSize
                };

                var response = new ApiResponseDto<PaginationResultDto<BrandResponseDto>>(
                    200,
                    "Brands retrieved successfully with pagination",
                    pagedResultDto
                );

                return Ok(response);
            }
            else
            {
                // Return all data without pagination
                var brands = await _service.GetAllBrandsAsync();
                var brandDtos = _mapper.Map<IEnumerable<BrandResponseDto>>(brands);

                var response = new ApiResponseDto<IEnumerable<BrandResponseDto>>(
                    200,
                    "All brands retrieved successfully",
                    brandDtos
                );

                return Ok(response);
            }
        }
    }
}
