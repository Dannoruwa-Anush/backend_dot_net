namespace WebApplication1.DTOs.ResponseDto
{
    public class ElectronicItemResponseDto
    {
        public int E_ItemID { get; set; }

        public string E_ItemName { get; set; } = string.Empty;

        public decimal Price { get; set; } 
        
        public int QOH { get; set; }
        
        //FK
        public int BrandId { get; set; }

        //FK
        public int CategoryID { get; set; }
    }
}