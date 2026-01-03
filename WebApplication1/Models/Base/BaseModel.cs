using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Base
{
    public abstract class BaseModel
    {
        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        //for: Who created/updated tracking
        public int? CreatedByUserID { get; set; }
        [ForeignKey(nameof(CreatedByUserID))]
        public User? CreatedBy { get; set; }

        public int? UpdatedByUserID { get; set; }
        [ForeignKey(nameof(UpdatedByUserID))]
        public User? UpdatedBy { get; set; }
    }
}