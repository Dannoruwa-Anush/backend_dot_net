using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderElectronicItemResponseDto
    {
        public int OrderItemID { get; set; }

        public int Quantity { get; set; }

        public decimal ItemPrice { get; set; }

        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Pending;

        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Partially_Paid;

        // Include simplified info about FK: ElectronicItem 
        public required ElectronicItemResponseDto ElectronicItemResponseDto { get; set; }

        // FK: CustomerOrder 
        public int CustomerOrderID { get; set; }
    }
}