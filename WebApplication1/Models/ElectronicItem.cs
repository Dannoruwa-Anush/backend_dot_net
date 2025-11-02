using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    [Index(nameof(E_ItemName), IsUnique = true)]
    public class ElectronicItem
    {
        [Key]
        public int E_ItemID { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [MaxLength(100)]
        public string E_ItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "QOH is required")]
        [Range(0, int.MaxValue, ErrorMessage = "QOH cannot be negative")]
        public int QOH { get; set; }

        // Store relative image path (e.g., "uploads/images/abc.jpg")
        [MaxLength(255)]
        public string? E_ItemImage { get; set; }

        // Not mapped to DB — only for file upload
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        //for: creation/modification tracking
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        //******* [Start: Brand (1) — ElectronicItems (M)] ****
        //FK
        public int BrandID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(BrandID))]
        public required Brand Brand { get; set; }
        //******* [End: Brand (1) — ElectronicItems (M)] ********


        //******* [Start: Category (1) — ElectronicItems (M)] ****
        //FK
        public int CategoryID { get; set; }

        // Many Side: Navigation property
        [ForeignKey(nameof(CategoryID))]
        public required Category Category { get; set; }
        //******* [End: Category (1) — ElectronicItems (M)] ******


        //******* [Start: ElectronicItems (1) — CustomerOrderElectronicItem(M)] ******
        // Many Side: Navigation property
        [InverseProperty(nameof(CustomerOrderElectronicItem.ElectronicItem))]
        public ICollection<CustomerOrderElectronicItem> CustomerOrderElectronicItems { get; set; } = new List<CustomerOrderElectronicItem>();
        //******* [End: ElectronicItems (1) — CustomerOrderElectronicItem(M)] ********
    }
}