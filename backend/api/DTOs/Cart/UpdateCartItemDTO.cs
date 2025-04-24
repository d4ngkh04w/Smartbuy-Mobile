using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Cart
{
    public class UpdateCartItemDTO
    {
        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}