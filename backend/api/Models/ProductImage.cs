using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("product_images")]
    public class ProductImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string ImagePath { get; set; } = string.Empty;
        
        public bool IsMain { get; set; } = false;
        
        // Change relationship from Product to ProductColor
        public int ColorId { get; set; }

        [ForeignKey(nameof(ColorId))]
        public ProductColor? Color { get; set; } = null;
    }
}