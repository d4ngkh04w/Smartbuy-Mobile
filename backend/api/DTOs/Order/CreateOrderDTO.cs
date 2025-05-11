using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Order
{
    public class CreateOrderDTO
    {
        [Required]
        public string PaymentMethod { get; set; } = "COD";

        [Required]
        public ICollection<CreateOrderItemDTO> Items { get; set; } = new List<CreateOrderItemDTO>();
    }
}