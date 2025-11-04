using WebApplication1.DTOs.ResponseDto.Base;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CategoryResponseDto : BaseResponseDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}