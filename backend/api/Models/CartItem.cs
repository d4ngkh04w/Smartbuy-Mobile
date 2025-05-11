using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("cart_items")]
    public class CartItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int ColorId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [ForeignKey(nameof(ColorId))]
        public ProductColor? Color { get; set; }

        public Guid CartId { get; set; }

        [ForeignKey(nameof(CartId))]
        public Cart? Cart { get; set; }
    }
}