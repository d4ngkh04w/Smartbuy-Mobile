using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("product_tags")]
    public class ProductTag
    {
        [Key]
        [Column(Order = 1)]
        public int ProductId { get; set; }
        
        [Key]
        [Column(Order = 2)]
        public int TagId { get; set; }

        [ForeignKey(nameof(ProductId))]
        [InverseProperty("ProductTags")]
        public Product? Product { get; set; } = null;

        [ForeignKey(nameof(TagId))]
        [InverseProperty("ProductTags")]
        public Tag? Tag { get; set; } = null;
    }
}