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
        public int ColorId { get; set; }
        public string ColorName { get; set; } = string.Empty;
        public string ColorImage { get; set; } = string.Empty;
    }
}