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

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal ImportPrice { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "text")]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(2, 1)")]
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public int Sold { get; set; }
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "timestamp")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        public int ProductLineId { get; set; }

        [InverseProperty("Products")]
        [ForeignKey(nameof(ProductLineId))]
        public ProductLine? ProductLine { get; set; } = null;

        [InverseProperty("Product")]
        public ICollection<ProductColor> Colors { get; set; } = new HashSet<ProductColor>();

        [InverseProperty("Product")]
        public ICollection<ProductImage> Images { get; set; } = new HashSet<ProductImage>();

        [InverseProperty("Product")]
        public ICollection<ProductDiscount> Discounts { get; set; } = new HashSet<ProductDiscount>();

        [InverseProperty("Product")]
        public ProductDetail? Detail { get; set; } = null;
        
        [InverseProperty("Product")]
        public ICollection<ProductTag> ProductTags { get; set; } = new HashSet<ProductTag>();
    }
}