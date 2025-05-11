using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Order
{
    public class CreateOrderItemDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
        
        [Required]
        public int ColorId { get; set; }
    }
}