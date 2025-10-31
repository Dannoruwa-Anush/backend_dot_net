using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class TransactionResponseDto
    {
        public int TransactionID { get; set; }

        public decimal AmountPaid { get; set; }

        public DateTime TransactionDate { get; set; }

        public TransactionStatusEnum TransactionStatus { get; set; } = TransactionStatusEnum.Paid;

        public DateTime? RefundDate { get; set; }
        
        // Include simplified info about FK: Order 
        public required CustomerOrderResponseDto CustomerOrderResponseDto{ get; set; }
    }
}