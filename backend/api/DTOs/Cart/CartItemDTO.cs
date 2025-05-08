using api.DTOs.Product;

namespace api.DTOs.Cart
{
    public class CartItemDTO
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public int ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string ColorImage { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public ProductDTO? Product { get; set; }
        public decimal SubTotal { get; set; }
    }
}