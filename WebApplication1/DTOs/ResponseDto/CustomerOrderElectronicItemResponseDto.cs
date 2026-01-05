namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderElectronicItemResponseDto
    {
        public int OrderItemID { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }

        // Include simplified info about FK: ElectronicItem 
        public required ElectronicItemResponseDto ElectronicItemResponseDto { get; set; }
    }
}