using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Order
{
    public class CreateOrderDTO
    {
        [Required]
        public string PaymentMethod { get; set; } = "COD";

        [Required]
        public decimal ShippingFee { get; set; } = 0;

        [Required]
        public ICollection<CreateOrderItemDTO> Items { get; set; } = new List<CreateOrderItemDTO>();
    }
}