namespace WebApplication1.DTOs.ResponseDto
{
    public class PaymentResponseDto
    {
        public int PaymentID { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal AmountPaid { get; set; }
        
        //FK
        public int OrderID { get; set; }

        //Fk Associated fields from Order (customer name, email)
    }
}