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

        private readonly IMapper _mapper;

        // Constructor
        public ElectronicItemController(IElectronicItemService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

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
        public async Task<IActionResult> Create([FromBody] ElectronicItemRequestDto electronicItemCreateDto)
        {
            var electronicItem = _mapper.Map<ElectronicItem>(electronicItemCreateDto);
            var created = await _service.AddElectronicItemAsync(electronicItem);
            var dto = _mapper.Map<ElectronicItemResponseDto>(created);

            var response = new ApiResponseDto<ElectronicItemResponseDto>(201, "Electronic item created successfully", dto);

            return CreatedAtAction(nameof(GetById), new { id = dto.E_ItemID}, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ElectronicItemRequestDto electronicItemUpdateDto)
        {
            try
            {
                var electronicItem = _mapper.Map<ElectronicItem>(electronicItemUpdateDto);
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
                    Items      = dtos,
                    TotalCount = pageResultDto.TotalCount,
                    PageNumber = pageResultDto.PageNumber,
                    PageSize   = pageResultDto.PageSize
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
                var dtos = _mapper.Map<IEnumerable<CustomerResponseDto>>(electronicItems);

                var response = new ApiResponseDto<IEnumerable<CustomerResponseDto>>(
                    200,
                    "All electronic items retrieved successfully",
                    dtos
                );

                return Ok(response);
            }
        }
    }
}