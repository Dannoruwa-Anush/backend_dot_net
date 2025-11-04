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
    [Route("api/[controller]")] //api/ElectronicItem 
    public class ElectronicItemController : ControllerBase
    {
        private readonly IElectronicItemService _service;

        private readonly IFileService _fileService;

        private readonly IMapper _mapper;

        // Constructor
        public ElectronicItemController(IElectronicItemService service, IFileService fileService, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _fileService = fileService;
            _mapper = mapper;
        }

        //CRUD operations
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var electronicItem = await _service.GetElectronicItemByIdAsync(id);
            if (electronicItem == null)
                return NotFound(new ApiResponseDto<string>(404, "Electronic item not found"));

            var dto = _mapper.Map<ElectronicItemResponseDto>(electronicItem);
            var response = new ApiResponseDto<ElectronicItemResponseDto>(200, "Electronic item retrieved successfully", dto);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ElectronicItemRequestDto electronicItemCreateDto)
        {
            var electronicItem = _mapper.Map<ElectronicItem>(electronicItemCreateDto);

            // Save image first, if exists
            if (electronicItemCreateDto.ImageFile != null)
            {
                electronicItem.ElectronicItemImage = await _fileService.SaveFileAsync(electronicItemCreateDto.ImageFile, "uploads/images");
            }

            var created = await _service.AddElectronicItemAsync(electronicItem);
            var dto = _mapper.Map<ElectronicItemResponseDto>(created);

            var response = new ApiResponseDto<ElectronicItemResponseDto>(201, "Electronic item created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.ElectronicItemID }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ElectronicItemRequestDto electronicItemUpdateDto)
        {
            try
            {
                var electronicItem = _mapper.Map<ElectronicItem>(electronicItemUpdateDto);

                // Load existing item to check for old image
                var existingItem = await _service.GetElectronicItemByIdAsync(id);
                if (existingItem == null)
                    return NotFound(new ApiResponseDto<string>(404, "Electronic item not found"));

                // Replace image if new one is uploaded
                if (electronicItemUpdateDto.ImageFile != null)
                {
                    // Delete old file
                    if (!string.IsNullOrEmpty(existingItem.ElectronicItemImage))
                        _fileService.DeleteFile(existingItem.ElectronicItemImage);

                    // Save new file
                    electronicItem.ElectronicItemImage = await _fileService.SaveFileAsync(electronicItemUpdateDto.ImageFile, "uploads/images");
                }
                else
                {
                    // Keep old image if no new file uploaded
                    electronicItem.ElectronicItemImage = existingItem.ElectronicItemImage;
                }

                var updated = await _service.UpdateElectronicItemAsync(id, electronicItem);
                var dto = _mapper.Map<ElectronicItemResponseDto>(updated);

                var response = new ApiResponseDto<ElectronicItemResponseDto>(200, "Electronic item updated successfully", dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Electronic item not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Electronic item with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var existingItem = await _service.GetElectronicItemByIdAsync(id);
                if (existingItem == null)
                    return NotFound(new ApiResponseDto<string>(404, "Electronic item not found"));

                // Delete image file
                if (!string.IsNullOrEmpty(existingItem.ElectronicItemImage))
                    _fileService.DeleteFile(existingItem.ElectronicItemImage);    

                await _service.DeleteElectronicItemAsync(id);

                var response = new ApiResponseDto<string>(204, "Electronic item deleted successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Electronic item not found"));

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

                var dtos = _mapper.Map<IEnumerable<ElectronicItemResponseDto>>(pageResultDto.Items);

                var paginationResult = new PaginationResultDto<ElectronicItemResponseDto>
                {
                    Items = dtos,
                    TotalCount = pageResultDto.TotalCount,
                    PageNumber = pageResultDto.PageNumber,
                    PageSize = pageResultDto.PageSize
                };

                var response = new ApiResponseDto<PaginationResultDto<ElectronicItemResponseDto>>(
                    200,
                    "Electronic items retrieved successfully with pagination",
                    paginationResult
                );

                return Ok(response);
            }
            else
            {
                // Return all data without pagination
                var electronicItems = await _service.GetAllElectronicItemsAsync();
                var dtos = _mapper.Map<IEnumerable<ElectronicItemResponseDto>>(electronicItems);

                var response = new ApiResponseDto<IEnumerable<ElectronicItemResponseDto>>(
                    200,
                    "All electronic items retrieved successfully",
                    dtos
                );

                return Ok(response);
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetAllByCategoryId(int categoryId)
        {
            var items = await _service.GetAllElectronicItemsByCategoryIdAsync(categoryId);
            if (items == null || !items.Any())
                return NotFound(new ApiResponseDto<string>(404, "No electronic items found for this category."));

            var dtos = _mapper.Map<IEnumerable<ElectronicItemResponseDto>>(items);
            var response = new ApiResponseDto<IEnumerable<ElectronicItemResponseDto>>(200, "Electronic items retrieved successfully", dtos);

            return Ok(response);
        }

        [HttpGet("brand/{brandId}")]
        public async Task<IActionResult> GetAllByBrandId(int brandId)
        {
            var items = await _service.GetAllElectronicItemsByBrandIdAsync(brandId);
            if (items == null || !items.Any())
                return NotFound(new ApiResponseDto<string>(404, "No electronic items found for this brand."));

            var dtos = _mapper.Map<IEnumerable<ElectronicItemResponseDto>>(items);
            var response = new ApiResponseDto<IEnumerable<ElectronicItemResponseDto>>(200, "Electronic items retrieved successfully", dtos);

            return Ok(response);
        }
    }
}