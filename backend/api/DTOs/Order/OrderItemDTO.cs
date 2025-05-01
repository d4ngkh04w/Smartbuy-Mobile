using api.DTOs.Product;

namespace api.DTOs.Order
{
    public class OrderItemDTO
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public int ProductId { get; set; }
        public ProductDTO? Product { get; set; }
    }
}