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
    [Route("api/[controller]")] //api/category 
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _service;

        private readonly IMapper _mapper;

        // Constructor
        public CategoryController(ICategoryService service, IMapper mapper)
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
            var categories = await _service.GetAllCategoriesAsync();
            if (categories == null || !categories.Any())
                return NotFound(new ApiResponseDto<string>(404, "Categories not found"));

            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
            var response = new ApiResponseDto<IEnumerable<CategoryResponseDto>>(
                200,
                "All categories retrieved successfully",
                responseDtos
            );
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // JWT is not required
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new ApiResponseDto<string>(404, "Category not found"));

            // Model -> ResponseDto   
            var responseDto = _mapper.Map<CategoryResponseDto>(category);
            var response = new ApiResponseDto<CategoryResponseDto>(200, "Categories retrieved successfully", responseDto);

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)] // JWT is required
        public async Task<IActionResult> Create([FromBody] CategoryRequestDto categoryCreateDto)
        {
            try
            {
                // RequestDto -> Nodel   
                var category = _mapper.Map<Category>(categoryCreateDto);
                var created = await _service.AddCategoryWithSaveAsync(category);

                // Model -> ResponseDto   
                var responseDto = _mapper.Map<CategoryResponseDto>(created);
                var response = new ApiResponseDto<CategoryResponseDto>(201, "Category created successfully", responseDto);
                return CreatedAtAction(nameof(GetById), new { id = responseDto.CategoryID }, response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Category with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)] // JWT is required
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDto categoryUpdateDto)
        {
            try
            {
                // RequestDto -> Model   
                var category = _mapper.Map<Category>(categoryUpdateDto);
                var updated = await _service.UpdateCategoryWithSaveAsync(id, category);

                // Model -> ResponseDto   
                var responseDto = _mapper.Map<CategoryResponseDto>(updated);
                var response = new ApiResponseDto<CategoryResponseDto>(200, "Category updated successfully", responseDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Category not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Category with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]  // JWT is required
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteCategoryWithSaveAsync(id);

                var response = new ApiResponseDto<string>(204, "Category deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Category not found"));

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
                var paginationResponse = _mapper.Map<PaginationResultDto<CategoryResponseDto>>(pageResultDto);
                var response = new ApiResponseDto<PaginationResultDto<CategoryResponseDto>>(
                    200,
                    "Categories retrieved successfully with pagination",
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