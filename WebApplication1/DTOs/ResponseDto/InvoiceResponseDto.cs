using WebApplication1.DTOs.ResponseDto.Base;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class InvoiceResponseDto : BaseResponseDto
    {
        public int InvoiceID { get; set; }

        public decimal InvoiceAmount { get; set; }

        public InvoiceTypeEnum InvoiceType { get; set; } = InvoiceTypeEnum.Bnpl_Initial_Pay;

        public InvoiceStatusEnum InvoiceStatus { get; set; } = InvoiceStatusEnum.Unpaid;

        public string? InvoiceFileUrl { get; set; }
        public string? ReceiptFileUrl { get; set; }
    }
}