using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Order
{
    public class CreateOrderDTO
    {
        [Required]
        public string PaymentMethod { get; set; } = "COD";

        // Loại bỏ ShippingFee từ DTO vì nó sẽ được tính toán trên server
        // Phí vận chuyển sẽ được tính dựa trên chính sách của cửa hàng

        [Required]
        public ICollection<CreateOrderItemDTO> Items { get; set; } = new List<CreateOrderItemDTO>();
    }
}