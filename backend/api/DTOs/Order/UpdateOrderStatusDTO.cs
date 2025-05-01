using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Order
{
    public class UpdateOrderStatusDTO
    {
        [Required]
        public string Status { get; set; } = string.Empty;

        public DateTime? DeliveryDate { get; set; }
    }
}