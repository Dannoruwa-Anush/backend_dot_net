using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CustomerOrderWithItemsResponseDto
    {
        //This ResponseDto will be used only with GetById
        public int OrderID { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public OrderPaymentStatusEnum OrderPaymentStatus { get; set; } = OrderPaymentStatusEnum.Partially_Paid;

        // Include simplified info about FK: Customer 
        public required CustomerResponseDto CustomerResponseDto { get; set; }

        // Include the list of related electronic items
        public ICollection<CustomerOrderElectronicItemResponseDto> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItemResponseDto>();
    }
}
