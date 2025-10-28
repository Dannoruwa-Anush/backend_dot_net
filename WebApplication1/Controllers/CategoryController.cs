using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService;

namespace WebApplication1.Controllers
{
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _service.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound(new ApiResponseDto<String>(404, "Category not found"));

            var dto = _mapper.Map<CategoryResponseDto>(category);
            var response = new ApiResponseDto<CategoryResponseDto>(200, "Categories retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequestDto categoryCreateDto)
        {
            var category = _mapper.Map<Category>(categoryCreateDto);
            var created = await _service.AddCategoryAsync(category);
            var dto = _mapper.Map<CategoryResponseDto>(created);

            var response = new ApiResponseDto<CategoryResponseDto>(201, "Category created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.CategoryID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDto categoryUpdateDto)
        {
            try
            {
                var category = _mapper.Map<Category>(categoryUpdateDto);
                var updated = await _service.UpdateCategoryAsync(id, category);
                var dto = _mapper.Map<CategoryResponseDto>(updated);

                var response = new ApiResponseDto<CategoryResponseDto>(200, "Category updated successfully", dto);
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.DeleteCategoryAsync(id);

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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? pageNumber, [FromQuery] int? pageSize)
        {
            if (pageNumber.HasValue && pageSize.HasValue && pageNumber > 0 && pageSize > 0)
            {
                // Call service to get paginated data
                var pageResultDto = await _service.GetAllWithPaginationAsync(pageNumber.Value, pageSize.Value);

                var dtos = _mapper.Map<IEnumerable<CategoryResponseDto>>(pageResultDto.Items);

                var paginationResult = new PaginationResultDto<CategoryResponseDto>
                {
                    Items      = dtos,
                    TotalCount = pageResultDto.TotalCount,
                    PageNumber = pageResultDto.PageNumber,
                    PageSize   = pageResultDto.PageSize
                };

                var response = new ApiResponseDto<PaginationResultDto<CategoryResponseDto>>(
                    200,
                    "Categories retrieved successfully with pagination",
                    paginationResult
                );

                return Ok(response);
            }
            else
            {
                // Return all data without pagination
                var Categories = await _service.GetAllCategoriesAsync();
                var dtos = _mapper.Map<IEnumerable<CategoryResponseDto>>(Categories);

                var response = new ApiResponseDto<IEnumerable<CategoryResponseDto>>(
                    200,
                    "All categories retrieved successfully",
                    dtos
                );

                return Ok(response);
            }
        }
    }
}