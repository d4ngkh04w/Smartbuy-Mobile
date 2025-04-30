using api.DTOs.Product;

namespace api.DTOs.Cart
{
    public class CartDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public IEnumerable<CartItemDTO> CartItems { get; set; } = new List<CartItemDTO>();
        public decimal TotalPrice { get; set; }
    }
}