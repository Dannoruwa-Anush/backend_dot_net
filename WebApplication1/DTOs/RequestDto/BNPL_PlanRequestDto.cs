using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.DTOs.RequestDto
{
    public class BNPL_PlanRequestDto
    {
        [Required(ErrorMessage = "Amount per installment is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Bnpl_AmountPerInstallment { get; set; }

        [Required(ErrorMessage = "Total installment count is required")]
        public int Bnpl_TotalInstallmentCount { get; set; }

        [Required(ErrorMessage = "Remaining installment count is required")]
        public int Bnpl_RemainingInstallmentCount { get; set; }
           
        //FK
        public int Bnpl_PlanTypeID { get; set; }

        //FK
        public int OrderID { get; set; }
    }
}