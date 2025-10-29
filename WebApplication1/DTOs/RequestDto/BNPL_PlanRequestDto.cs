using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto
{
    public class BNPL_PlanRequestDto
    {
        [Required(ErrorMessage = "Total installments are required")]
        public int Bnpl_TotalInstallments { get; set; }

        [Required(ErrorMessage = "Installment amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_InstallmentAmount { get; set; }

        [Required(ErrorMessage = "Interest rate is required")]
        public double Bnpl_InterestRate { get; set; }

        [Required(ErrorMessage = "Bnpl Start date is required")]
        public DateTime Bnpl_StartDate { get; set; }

        [Required(ErrorMessage = "Bnpl next due date is required")]
        public DateTime Bnpl_NextDueDate { get; set; }

        [Required(ErrorMessage = "Remaining balance is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_RemainingBalance { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(BnplStatusEnum))]
        public BnplStatusEnum Bnpl_Status { get; set; } = BnplStatusEnum.Incomplete;
           
        //FK
        public int Bnpl_PlanTypeID { get; set; }

        //FK
        public int OrderID { get; set; }
    }
}