using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs.RequestDto.Auth;
using WebApplication1.DTOs.ResponseDto.Auth;
using WebApplication1.DTOs.ResponseDto.Common;
using WebApplication1.Models;
using WebApplication1.Services.IService.Auth;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
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
            catch
            {
                return StatusCode(500, new ApiResponseDto<string>(500, "Internal server error"));
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                var token = await _service.LoginAsync(loginDto.Email, loginDto.Password);

                var responseDto = new LoginResponseDto
                {
                    Token = token,
                    Email = loginDto.Email,
                    Role = "Customer" 
                };

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