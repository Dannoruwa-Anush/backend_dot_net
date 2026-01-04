using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto
{
    public class CustomerOrderRequestDto
    {
        [Required(ErrorMessage = "Order source is required")]
        public OrderSourceEnum OrderSource { get; set; } = OrderSourceEnum.PhysicalShop;

        // Adding Electronic Items to the Order
        [Required(ErrorMessage = "At least one electronic item must be added")]
        public List<CustomerOrderElectronicItemRequestDto> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItemRequestDto>();

        //FK
        // CustomerID is nullable to support cashier's direct orders
        public int? CustomerID { get; set; }

        //FK : Only for BNPL orders
        public int? Bnpl_PlanTypeID { get; set; }

        public int? InstallmentCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? InitialPayment { get; set; }
    }
}