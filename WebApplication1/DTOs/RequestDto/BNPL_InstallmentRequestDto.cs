using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto
{
    public class BNPL_InstallmentRequestDto
    {
        [Required(ErrorMessage = "Installment No is required")]
        public int Bnpl_InstallmentNo { get; set; }

        [Required(ErrorMessage = "Installment due date is required")]
        public DateTime Bnpl_Installment_DueDate { get; set; }

        [Required(ErrorMessage = "Installment amount due is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_Installment_AmountDue { get; set; }

        [Required(ErrorMessage = "Installment amount paid is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_Installment_AmountPaid { get; set; }

        [Required(ErrorMessage = "Installment payment date is required")]
        public DateTime Bnpl_Installment_PaymentDate { get; set; }

        [Required(ErrorMessage = "Installment late interest is required")]
        public double Bnpl_Installment_LateInterest { get; set; }

        [Required(ErrorMessage = "Installment arrears carried is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_Installment_ArrearsCarried { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(BNPL_Installment_StatusEnum))]
        public BNPL_Installment_StatusEnum Bnpl_Installment_Status { get; set; } = BNPL_Installment_StatusEnum.Pending;

        //FK
        public int Bnpl_PlanID { get; set; }
    }
}