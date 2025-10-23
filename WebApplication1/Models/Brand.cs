using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    [Index(nameof(Name), IsUnique = true)] // Ensures uniqueness at DB level
    public class Brand
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; //initializes the string, avoids null.
    }
}