using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    [Index(nameof(BrandName), IsUnique = true)] // Ensures uniqueness at DB level
    public class Brand
    {
        [Key]
        public int BrandID { get; set; }

        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        //******* [Start: Brand (1) — ElectronicItems (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(ElectronicItem.Brand))]
        public ICollection<ElectronicItem> ElectronicItems { get; set; } = new List<ElectronicItem>();
        //******* [End: Brand (1) — ElectronicItems (M)] ******
    }
}