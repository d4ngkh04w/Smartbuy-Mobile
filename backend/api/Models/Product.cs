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

        [Column(TypeName = "decimal(12, 2)")]
        public decimal ImportPrice { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal SalePrice { get; set; }

        [Column(TypeName = "text")]
        public string? Description { get; set; }
        public int Sold { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ManuallyDeactivated { get; set; } = false;

        [Column(TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "timestamp")]
        public DateTime? UpdatedAt { get; set; }

        public int ProductLineId { get; set; }

        [ForeignKey(nameof(ProductLineId))]
        public ProductLine? ProductLine { get; set; } = null;

        public ICollection<ProductColor> Colors { get; set; } = new HashSet<ProductColor>();
        public ICollection<ProductDiscount> Discounts { get; set; } = new HashSet<ProductDiscount>();
        public ProductDetail? Detail { get; set; } = null;
        public ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        // public ICollection<ProductTag> ProductTags { get; set; } = new HashSet<ProductTag>();
    }
}