namespace api.DTOs.Product
{
    public class ProductSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; } // Giá sau khi giảm
        public decimal SalePrice { get; set; } // Giá gốc
        public string Discount { get; set; } = string.Empty; // Thông tin giảm giá
        public string ImageUrl { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public int Sold { get; set; }

    }
}