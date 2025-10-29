using System.ComponentModel.DataAnnotations;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Models
{
    public class BNPL_PLAN
    {
        [Key]
        public int Bnpl_PlanID { get; set; }

        public decimal Bnpl_TotalInstallments { get; set; }

        public decimal Bnpl_InstallmentAmount { get; set; }

        public double Bnpl_InterestRate { get; set; }

        public DateTime Bnpl_StartDate { get; set; }

        public DateTime Bnpl_NextDueDate { get; set; }

        public decimal Bnpl_RemainingBalance { get; set; }

        //InComplete : 0 (default value), Completed : 1, 
        public BnplStatusEnum Bnpl_Status { get; set; } = BnplStatusEnum.Incomplete;

        //FK : CustomerOrder

        //FK : BNPL_Plan_Type
    }
}