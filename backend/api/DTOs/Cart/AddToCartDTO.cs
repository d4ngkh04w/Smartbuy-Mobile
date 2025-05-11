using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Cart
{
    public class AddToCartDTO
    {
        [Required(ErrorMessage = "ProductId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than 0")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "ColorId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ColorId must be greater than 0")]
        public int ColorId { get; set; }
    }
}