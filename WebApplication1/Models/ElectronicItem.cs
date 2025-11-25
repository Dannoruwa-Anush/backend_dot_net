using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Base;

namespace WebApplication1.Models
{
    [Index(nameof(ElectronicItemName), IsUnique = true)]
    public class ElectronicItem : BaseModel //(In base model: CreatedAt, UpdatedAt)
    {
        [Key]
        public int ElectronicItemID { get; set; }

        [Required(ErrorMessage = "Item name is required")]
        [MaxLength(100)]
        public string ElectronicItemName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "QOH is required")]
        [Range(0, int.MaxValue, ErrorMessage = "QOH cannot be negative")]
        public int QOH { get; set; }

        // Store relative image path (e.g., "uploads/images/abc.jpg")
        [MaxLength(255)]
        public string? ElectronicItemImage { get; set; }

        // Not mapped to DB — only for file upload
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [ConcurrencyCheck]
        public byte[] RowVersion { get; set; }  = new byte[8]; // for optimistic concurrency.
        
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