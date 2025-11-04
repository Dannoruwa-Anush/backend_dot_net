namespace WebApplication1.DTOs.ResponseDto.Base
{
    public abstract class BaseResponseDto
    {
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}