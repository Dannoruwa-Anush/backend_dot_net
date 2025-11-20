using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.DTOs.RequestDto.StatusChange
{
    public class CustomerOrderStatusChangeRequestDto
    {        
        public int OrderID { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        [EnumDataType(typeof(OrderStatusEnum))]
        public OrderStatusEnum NewOrderStatus { get; set; } = OrderStatusEnum.Pending;
    }
}