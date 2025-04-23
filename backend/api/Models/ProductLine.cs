using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Table("product_lines")]
    [Index(nameof(Name), nameof(BrandId), IsUnique = true)]
    public class ProductLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "text")]
        public string? Description { get; set; } = string.Empty;

        [Column(TypeName = "varchar(255)")]
        public string Image { get; set; } = string.Empty;

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? UpdatedAt { get; set; }

        public int BrandId { get; set; }

        [ForeignKey(nameof(BrandId))]
        [InverseProperty("ProductLines")]
        public Brand? Brand { get; set; } = null;

        [InverseProperty("ProductLine")]
        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}