using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("product_tags")]
    public class ProductTag
    {
        [Key]
        public int ProductId { get; set; }

        [Key]
        public int TagId { get; set; }

        [ForeignKey(nameof(ProductId))]
        [InverseProperty("ProductTags")]
        public Product? Product { get; set; } = null;

        [ForeignKey(nameof(TagId))]
        [InverseProperty("ProductTags")]
        public Tag? Tag { get; set; } = null;
    }
}