using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.ResponseDto
{
    public class BNPL_InstallmentResponseDto
    {
        public int Bnpl_InstallmentID { get; set; }

        public int Bnpl_InstallmentNo { get; set; }

        public DateTime Bnpl_Installment_DueDate { get; set; }

        public decimal Bnpl_Installment_AmountDue { get; set; }

        public decimal Bnpl_Installment_AmountPaid { get; set; }

        public DateTime Bnpl_Installment_PaymentDate { get; set; }

        public double Bnpl_Installment_LateInterest { get; set; }

        public decimal Bnpl_Installment_ArrearsCarried { get; set; }

        public BNPL_Installment_StatusEnum Bnpl_Installment_Status { get; set; } = BNPL_Installment_StatusEnum.Pending;

        //FK
        public int Bnpl_PlanID { get; set; }
    }
}