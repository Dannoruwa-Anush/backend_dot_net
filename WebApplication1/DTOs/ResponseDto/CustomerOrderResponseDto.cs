using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderResponseDto
    {
        public int OrderID { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? ShippingDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Pending;

        public OrderPaymentStatusEnum PaymentStatus { get; set; } = OrderPaymentStatusEnum.Partially_Paid;

        // Include simplified info about FK: Customer 
        public required CustomerResponseDto CustomerResponseDto { get; set; }
    }
}