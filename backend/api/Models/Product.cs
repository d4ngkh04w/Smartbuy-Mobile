using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("products")]

    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // [Required]
        // [Column(TypeName = "varchar(100)")]
        // [StringLength(100, MinimumLength = 4, ErrorMessage = "Product name must be between 4 and 100 characters")]
        // [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Product name can only contain letters, numbers, and spaces")]
        // public string Name { get; set; } = null!;

        // [Required]
        // public int Quantity { get; set; }

        // [Required]
        // [Column(TypeName = "decimal(12, 2)")]
        // [Range(0, 9999999999.99, ErrorMessage = "Import price must be a positive number")]
        // [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Import price must be a valid decimal number with up to 2 decimal places")]
        // [DataType(DataType.Currency)]
        // public decimal ImportPrice { get; set; }

        // [Required]
        // [Column(TypeName = "decimal(12, 2)")]
        // [Range(0, 9999999999.99, ErrorMessage = "Sale price must be a positive number")]
        // [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Sale price must be a valid decimal number with up to 2 decimal places")]
        // [DataType(DataType.Currency)]
        // public decimal SalePrice { get; set; }

        // [Required]
        // [Column(TypeName = "text")]
        // [StringLength(65535, ErrorMessage = "Description too long")]
        // public string Description { get; set; } = null!;

        // [Required]
        // [Column(TypeName = "decimal(2, 1)")]
        // [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        // [RegularExpression(@"^[0-5](\.[0-9])?$", ErrorMessage = "Rating format is invalid")]
        // public decimal Rating { get; set; }

        // [Required]
        // [Column(TypeName = "int")]
        // [Range(0, int.MaxValue, ErrorMessage = "Rating count must be a non-negative integer")]
        // [RegularExpression(@"^\d+$", ErrorMessage = "Rating count format is invalid")]
        // public int RatingCount { get; set; }
        // public int Sold { get; set; }
        // public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? CategoryId { get; set; }
        // public int ColorId { get; set; }
        public virtual Category? Category { get; set; }
        // public Color Color { get; set; } = null!;
        // public ICollection<ProductImage> ProductImages { get; set; } = new HashSet<ProductImage>();
    }
}