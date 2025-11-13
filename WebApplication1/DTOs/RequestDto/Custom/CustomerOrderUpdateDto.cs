using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto.Custom
{
    public class CustomerOrderUpdateDto
    {
        public OrderStatusEnum? OrderStatus { get; set; }
        public OrderPaymentStatusEnum? PaymentStatus { get; set; }
    }
}