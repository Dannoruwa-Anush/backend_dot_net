using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto
{
    public class CustomerOrderRequestDto
    {
        [Required(ErrorMessage = "Order date is required")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Total amount is required")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(PaymentStatusEnum))]
        public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Partially_Paid;

        //FK
        public int CustomerID { get; set; }

        // Adding Electronic Items to the Order
        [Required(ErrorMessage = "At least one electronic item must be added")]
        public List<CustomerOrderElectronicItemRequestDto> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItemRequestDto>();
    }
}