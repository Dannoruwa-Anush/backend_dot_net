namespace WebApplication1.DTOs.ResponseDto
{
    public class ElectronicItemResponseDto
    {
        public int ElectronicItemID { get; set; }

        public string ElectronicItemName { get; set; } = string.Empty;

        public decimal Price { get; set; } 
        
        public int QOH { get; set; }

        // full URL in response
        public string? ElectronicItemImageUrl { get; set; }
        
        // Store filename in DB
        public string? ElectronicItemImage { get; set; }
        
        // Include simplified info about FK: Brand 
        public required BrandResponseDto BrandResponseDto { get; set; } 

        // Include simplified info about FK: Category 
        public required CategoryResponseDto CategoryResponseDto { get; set; }
    }
}