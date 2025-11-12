using WebApplication1.DTOs.ResponseDto.Base;

namespace WebApplication1.DTOs.ResponseDto
{
    public class UserResponseDto : BaseResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}