namespace WebApplication1.DTOs.RequestDto
{
    public class PhysicalShopSessionRequestDto
    {
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public bool IsActive { get; set; } = false;
    }
}