using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    [Index(nameof(CategoryName), IsUnique = true)]
    public class Category
    {
        [Key]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        //******* [Start: Category (1) — ElectronicItems (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(ElectronicItem.Category))]
        public ICollection<ElectronicItem> ElectronicItems { get; set; } = new List<ElectronicItem>();
        //******* [End: Category (1) — ElectronicItems (M)] ******
    } 
}