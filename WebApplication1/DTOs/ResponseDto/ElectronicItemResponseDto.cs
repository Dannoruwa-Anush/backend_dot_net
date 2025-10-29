namespace WebApplication1.DTOs.ResponseDto
{
    public class ElectronicItemResponseDto
    {
        public int E_ItemID { get; set; }

        public string E_ItemName { get; set; } = string.Empty;

        public decimal Price { get; set; } 
        
        public int QOH { get; set; }
        
        // Include simplified info about FK: Brand 
        public required BrandResponseDto BrandResponseDto { get; set; } 

        // Include simplified info about FK: Category 
        public required CategoryResponseDto CategoryResponseDto { get; set; }
    }
}