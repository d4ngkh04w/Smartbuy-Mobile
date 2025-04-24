using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("carts")]
    public class Cart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid UserId { get; set; }

        [InverseProperty("Cart")]
        public ICollection<CartItem> CartItems { get; set; } = new HashSet<CartItem>();
    }
}