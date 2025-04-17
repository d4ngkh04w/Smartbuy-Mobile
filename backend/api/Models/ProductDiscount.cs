using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("product_discounts")]
    public class ProductDiscount
    {
        public int ProductId { get; set; }
        public int DiscountId { get; set; }

        [InverseProperty("Discounts")]
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; } = null;

        [InverseProperty("Products")]
        [ForeignKey(nameof(DiscountId))]
        public Discount? Discount { get; set; } = null;
    }
}