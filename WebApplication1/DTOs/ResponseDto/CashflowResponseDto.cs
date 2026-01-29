using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class CashflowResponseDto
    {
        public int CashflowID { get; set; }

        public decimal AmountPaid { get; set; }

        public string CashflowRef { get; set; } = string.Empty;

        public DateTime CashflowDate { get; set; }
        
        public CashflowPaymentNatureEnum CashflowPaymentNature { get; set; } = CashflowPaymentNatureEnum.Payment;
        
        // Include simplified info about FK: Order 
        public required CustomerOrderResponseDto CustomerOrderResponseDto{ get; set; }
    }
}