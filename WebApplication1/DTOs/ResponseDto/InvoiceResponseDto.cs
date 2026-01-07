using WebApplication1.DTOs.ResponseDto.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class InvoiceResponseDto : BaseResponseDto
    {
        public int InvoiceID { get; set; }

        public decimal InvoiceAmount { get; set; }

        // For bnpl installment payment
        public int? InstallmentNo { get; set; }

        public InvoiceTypeEnum InvoiceType { get; set; } = InvoiceTypeEnum.Bnpl_Initial_Payment_Invoice;

        public InvoiceStatusEnum InvoiceStatus { get; set; } = InvoiceStatusEnum.Opened;

        public string? InvoiceFileUrl { get; set; }

        // Include simplified info about FK: Order
        public required CustomerOrderResponseDto CustomerOrderResponseDto { get; set; }

        // Include simplified info about FK: Cashflow
        public CashflowResponseDto? CashflowResponseDto { get; set; }
    }
}