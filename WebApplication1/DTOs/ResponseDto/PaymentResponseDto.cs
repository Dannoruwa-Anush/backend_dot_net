namespace WebApplication1.DTOs.ResponseDto
{
    public class PaymentResponseDto
    {
        public int PaymentID { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal AmountPaid { get; set; }
        
        // Include simplified info about FK: Order 
        public required CustomerOrderResponseDto CustomerOrderResponseDto{ get; set; }
    }
}