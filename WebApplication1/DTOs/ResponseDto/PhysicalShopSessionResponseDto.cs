using WebApplication1.DTOs.ResponseDto.Base;

namespace WebApplication1.DTOs.ResponseDto
{
    public class PhysicalShopSessionResponseDto : BaseResponseDto
    {
        public int PhysicalShopSessionID { get; set; }
        
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public bool IsActive { get; set; } = false;
    }
}