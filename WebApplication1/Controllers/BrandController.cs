using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto.Common;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Utils.Settings;

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
            _mapper  = mapper;
        }

        //CRUD operations
        [HttpGet]
        [AllowAnonymous] // JWT is not required
        public async Task<IActionResult> GetAll()
        {
            var brands = await _service.GetAllBrandsAsync();
            if (brands == null || !brands.Any())
                return NotFound(new ApiResponseDto<string>(404, "Brands not found"));
            
            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<BrandResponseDto>>(brands);
            var response = new ApiResponseDto<IEnumerable<BrandResponseDto>>(
                200,
                "All brands retrieved successfully",
                responseDtos
            );
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // JWT is not required
        public async Task<IActionResult> GetById(int id)
        {
            var brand = await _service.GetBrandByIdAsync(id);
            if (brand == null)
                return NotFound(new ApiResponseDto<string>(404, "Brand not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<BrandResponseDto>(brand);
            var response = new ApiResponseDto<BrandResponseDto>(200, "Brand retrieved successfully", responseDto);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.AdminOrManager)]  // JWT is required
        public async Task<IActionResult> Create([FromBody] BrandRequestDto brandCreateDto)
        {
            try
            {   // RequestDto -> Model
                var brand = _mapper.Map<Brand>(brandCreateDto);
                var created = await _service.AddBrandWithSaveAsync(brand);

                // Model -> ResponseDto
                var responseDto = _mapper.Map<BrandResponseDto>(created);
                var response = new ApiResponseDto<BrandResponseDto>(201, "Brand created successfully", responseDto);
                return CreatedAtAction(nameof(GetById), new { id = responseDto.BrandID }, response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Brand with the given name already exists"));

                // Generic error
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOrManager)]  // JWT is required
        public async Task<IActionResult> Update(int id, [FromBody] BrandRequestDto brandUpdateDto)
        {
            try
            {
                // RequestDto -> Model
                var brand = _mapper.Map<Brand>(brandUpdateDto);
                var updated = await _service.UpdateBrandWithSaveAsync(id, brand);

                // Model -> ResponseDto
                var responseDto = _mapper.Map<BrandResponseDto>(updated);
                var response = new ApiResponseDto<BrandResponseDto>(200, "Brand updated successfully", responseDto);
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
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]  // JWT is required
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteBrandWithSaveAsync(id);

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
        [HttpGet("paged")]
        [AllowAnonymous] // JWT is not required
        public async Task<IActionResult> GetAllWithPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, string? searchKey = null)
        {
            try
            {
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber, pageSize, searchKey);

                // Model -> ResponseDto   
                var paginationResponse = _mapper.Map<PaginationResultDto<BrandResponseDto>>(pageResultDto);            
                var response = new ApiResponseDto<PaginationResultDto<BrandResponseDto>>(
                    200,
                    "Brands retrieved successfully with pagination",
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
