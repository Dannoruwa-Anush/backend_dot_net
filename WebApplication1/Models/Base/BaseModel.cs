namespace WebApplication1.Models.Base
{
    public abstract class BaseModel
    {
        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}