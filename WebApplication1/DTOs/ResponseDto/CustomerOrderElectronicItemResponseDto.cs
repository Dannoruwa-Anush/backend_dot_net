namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderElectronicItemResponseDto
    {
        public int OrderItemID { get; set; }

        public int Quantity { get; set; }

        public decimal ItemPrice { get; set; }

        //FK
        public int E_ItemID { get; set; }


        //FK
        public int OrderID { get; set; }
    }
}