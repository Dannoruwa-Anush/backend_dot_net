using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using WebApplication1.Services.IService;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.DTOs.ResponseDto.Common;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Utils.Settings;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")] //api/physicalShopSession 
    public class PhysicalShopSessionController : ControllerBase
    {
        private readonly IPhysicalShopSessionService _service;
        private readonly IMapper _mapper;

        // Constructor
        public PhysicalShopSessionController(IPhysicalShopSessionService service, IMapper mapper)
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
            var sessions = await _service.GetAllPhysicalShopSessionsAsync();
            if (sessions == null || !sessions.Any())
                return NotFound(new ApiResponseDto<string>(404, "Physical Shop Sessions not found"));
            
            // Model -> ResponseDto
            var responseDtos = _mapper.Map<IEnumerable<PhysicalShopSessionResponseDto>>(sessions);
            var response = new ApiResponseDto<IEnumerable<PhysicalShopSessionResponseDto>>(
                200,
                "All physical Shop Sessions retrieved successfully",
                responseDtos
            );
            return Ok(response);
        }

        [HttpGet("{id}")]
        [AllowAnonymous] // JWT is not required
        public async Task<IActionResult> GetById(int id)
        {
            var session = await _service.GetPhysicalShopSessionByIdAsync(id);
            if (session == null)
                return NotFound(new ApiResponseDto<string>(404, "Physical shop session not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<PhysicalShopSessionResponseDto>(session);
            var response = new ApiResponseDto<PhysicalShopSessionResponseDto>(200, "Physical shop session retrieved successfully", responseDto);
            return Ok(response);
        }

        [HttpGet("today-active")]
        [AllowAnonymous] // JWT is not required
        public async Task<IActionResult> GeActiveForToday()
        {
            var session = await _service.GetActivePhysicalShopSessionForTodayAsync();
            if (session == null)
                return NotFound(new ApiResponseDto<string>(404, "Physical shop session not found for today"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<PhysicalShopSessionResponseDto>(session);
            var response = new ApiResponseDto<PhysicalShopSessionResponseDto>(200, "Physical shop session for today retrieved successfully", responseDto);
            return Ok(response);
        }

        [HttpGet("latest-active")]
        [AllowAnonymous] // JWT is not required
        public async Task<IActionResult> GeLatestActive()
        {
            var session = await _service.GetLatestActivePhysicalShopSessionAsync();
            if (session == null)
                return NotFound(new ApiResponseDto<string>(404, "Latest physical shop session not found"));

            // Model -> ResponseDto
            var responseDto = _mapper.Map<PhysicalShopSessionResponseDto>(session);
            var response = new ApiResponseDto<PhysicalShopSessionResponseDto>(200, "Latest physical shop session retrieved successfully", responseDto);
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] // JWT is required
        public async Task<IActionResult> Create()
        {
            try
            { 
                var created = await _service.AddPhysicalShopSessionWithSaveAsync();

                var response = new ApiResponseDto<PhysicalShopSessionResponseDto>(201, "Physical shop session created successfully");
                return CreatedAtAction(nameof(GetById), response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Physical shop session with the given name already exists"));

                // Generic error
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = AuthorizationPolicies.ManagerOnly)]  // JWT is required
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var updated = await _service.ModifyPhysicalShopSessionWithTransactionAsync(id);

                var response = new ApiResponseDto<PhysicalShopSessionResponseDto>(200, "Physical shop session updated successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "Physical shop session not found"));

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new ApiResponseDto<string>(400, "Physical shop session with the given name already exists"));

                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }
    }
}
