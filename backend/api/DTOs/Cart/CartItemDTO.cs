using api.DTOs.Product;

namespace api.DTOs.Cart
{
    public class CartItemDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public ProductDTO? Product { get; set; }
        public decimal SubTotal { get; set; }
    }
}