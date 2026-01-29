using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Audit
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public string Action { get; set; } = null!; // Added / Modified / Deleted
        public string EntityName { get; set; } = null!;
        public int EntityId { get; set; } // Primary key of entity

        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Position { get; set; } = null!;

        [Column(TypeName = "LONGTEXT CHARACTER SET utf8mb4")]
        public string Changes { get; set; } = null!; // JSON of before/after

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}