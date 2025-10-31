using System.ComponentModel.DataAnnotations;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto
{
    public class CashflowRequestDto
    {
        [Required(ErrorMessage = "Order ID is required")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Amount paid is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount paid must be greater than zero")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Cashflow type is required")]
        public CashflowTypeEnum Type { get; set; }

        public int? InstallmentNo { get; set; }

        public bool IsFullInstallmentPayment { get; set; } = false;
    }
}