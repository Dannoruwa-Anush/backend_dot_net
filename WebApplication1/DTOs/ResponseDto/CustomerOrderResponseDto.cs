using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderResponseDto
    {
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Pending;

        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Partially_Paid;

        // Include simplified info about FK: Customer 
        public required CustomerResponseDto CustomerResponseDto { get; set; }
    }
}