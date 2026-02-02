using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto
{
    public class CustomerOrderRequestDto
    {
        [Required(ErrorMessage = "Order source is required")]
        public OrderSourceEnum OrderSource { get; set; } = OrderSourceEnum.PhysicalShop;

        [Required(ErrorMessage = "Order payment mode is required")]
        public OrderPaymentModeEnum OrderPaymentMode { get; set; } = OrderPaymentModeEnum.Pay_Bnpl;

        // Adding Electronic Items to the Order
        [Required(ErrorMessage = "At least one electronic item must be added")]
        public List<CustomerOrderElectronicItemRequestDto> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItemRequestDto>();

        //Fk
        public int? PhysicalShopSessionId { get; set; }

        //FK
        // CustomerID is nullable to support manager's direct orders
        // Used ONLY when OrderSource == PhysicalShop
        public int? PhysicalShopBillToCustomerID { get; set; }
        // OnlineShop: CustomerID will be handle with JWT token

        //FK : Only for BNPL orders
        public int? Bnpl_PlanTypeID { get; set; }

        public int? Bnpl_InstallmentCount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Bnpl_InitialPayment { get; set; }
    }
}