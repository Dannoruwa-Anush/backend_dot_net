using WebApplication1.DTOs.ResponseDto.Base;

namespace WebApplication1.DTOs.ResponseDto
{
    public class BrandResponseDto : BaseResponseDto
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; } = string.Empty;
    }
}
