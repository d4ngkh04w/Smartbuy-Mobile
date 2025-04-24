using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("cart_items")]
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int CartId { get; set; }

        [ForeignKey("CartId")]
        [InverseProperty("CartItems")]
        public Cart? Cart { get; set; }
    }
}