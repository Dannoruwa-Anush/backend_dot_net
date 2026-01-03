using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models.Base;

namespace WebApplication1.Models
{
    [Index(nameof(BrandName), IsUnique = true)] // Ensures uniqueness at DB level
    public class Brand : BaseModel //(In base model: Audit fields)
    {
        [Key]
        public int BrandID { get; set; }

        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(100)]
        public string BrandName { get; set; } = string.Empty;

        //******* [Start: Brand (1) — ElectronicItems (M)] ****
        // One Side: Navigation property
        [InverseProperty(nameof(ElectronicItem.Brand))]
        public ICollection<ElectronicItem> ElectronicItems { get; set; } = new List<ElectronicItem>();
        //******* [End: Brand (1) — ElectronicItems (M)] ******
    }
}