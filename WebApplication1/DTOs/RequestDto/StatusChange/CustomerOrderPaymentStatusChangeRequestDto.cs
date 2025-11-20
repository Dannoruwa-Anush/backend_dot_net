using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto.StatusChange
{
    public class CustomerOrderPaymentStatusChangeRequestDto
    {        
        public int OrderID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderStatusEnum))]
        public OrderPaymentStatusEnum NewPaymentStatus { get; set; } = OrderPaymentStatusEnum.Partially_Paid;
    }
}