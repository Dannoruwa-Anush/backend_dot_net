using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Base;

namespace WebApplication1.Models
{
    public class PhysicalShopSession : BaseModel //(In base model: Audit fields)
    {
        [Key]
        public int PhysicalShopSessionID { get; set; }

        // Business session time
        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public bool IsActive { get; set; } = false;

        //******* [Start: CustomerOrder (M) - PhysicalShopSession (0..1)] ******
        // One Side: Navigation property
        [InverseProperty(nameof(CustomerOrder.PhysicalShopSession))]
        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
        //******* [End: CustomerOrder (M) - PhysicalShopSession (0..1)] ******
    }
}