using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    [Index(nameof(CategoryName), IsUnique = true)]
    public class Category
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;
    } 
}