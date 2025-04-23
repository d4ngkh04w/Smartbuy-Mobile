using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("product_discounts")]
    public class ProductDiscount
    {
        [Key]
        public int ProductId { get; set; }

        [Key]
        public int DiscountId { get; set; }

        [InverseProperty("Discounts")]
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; } = null;

        [InverseProperty("Products")]
        [ForeignKey(nameof(DiscountId))]
        public Discount? Discount { get; set; } = null;
    }
}