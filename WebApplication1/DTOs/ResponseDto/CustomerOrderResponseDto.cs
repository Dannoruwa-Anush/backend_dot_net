using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderResponseDto
    {
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Partially_Paid;

        //FK
        public int CustomerID { get; set; }

        //customer details
    }
}