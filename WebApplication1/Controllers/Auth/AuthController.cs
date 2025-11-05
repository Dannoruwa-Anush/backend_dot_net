using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.Auth;
using WebApplication1.DTOs.ResponseDto.Auth;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService.Auth;

namespace WebApplication1.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // This controller don't require JWT
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _service;
        private readonly IMapper _mapper;

        // Constructor
        public AuthController(IAuthService service, IMapper mapper)
        {
            // Dependency injection
            _service = service;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponseDto<string>(400, "Invalid data"));

            try
            {
                var user = _mapper.Map<User>(dto);
                var created = await _service.RegisterUserAsync(user);

                var responseDto = _mapper.Map<LoginResponseDto>(created);
                var response = new ApiResponseDto<LoginResponseDto>(201, "User registered successfully", responseDto);
                return CreatedAtAction(nameof(Register), response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponseDto<string>(400, ex.Message));
            }
            catch (Exception ex)
            {
                var message = ex.Message;

                if (message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new ApiResponseDto<string>(404, "User with the given email already exists"));

                // Generic error
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred. Please try again later."));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                // Login returns (User, JWT)
                var (user, token) = await _service.LoginAsync(loginDto.Email, loginDto.Password);

                // Map User -> LoginResponseDto
                var responseDto = _mapper.Map<LoginResponseDto>(user);
                responseDto.Token = token; // Set JWT token separately

                var response = new ApiResponseDto<LoginResponseDto>(200, "Login successful", responseDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponseDto<string>(401, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "An internal server error occurred."));
            }
        }
    }
}