using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderResponseDto
    {
        public int OrderID { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime? ShippedDate { get; set; }

        public DateTime? DeliveredDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Pending;

        public DateTime? PaymentCompletedDate { get; set; }

        public OrderPaymentStatusEnum OrderPaymentStatus { get; set; } = OrderPaymentStatusEnum.Partially_Paid;

        // Include simplified info about FK: Customer 
        // CustomerID is nullable to support cashier's direct orders
        public CustomerResponseDto? CustomerResponseDto { get; set; }

        // Include simplified info about child items: CustomerOrderElectronicItem 
        public ICollection<CustomerOrderElectronicItemResponseDto> CustomerOrderElectronicItemResponseDto { get; set; } = new List<CustomerOrderElectronicItemResponseDto>();
    
        // Include simplified info about child items: Invoice
        // Only for draft invoice 
        public InvoiceResponseDto? CurrentInvoice { get; set; }
    }
}