namespace api.DTOs.Product
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public int Sold { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int ProductLineId { get; set; }
        public string ProductLineName { get; set; } = string.Empty;
        public ICollection<ProductColorDTO> Colors { get; set; } = new HashSet<ProductColorDTO>();
        public ICollection<ProductImageDTO> Images { get; set; } = new HashSet<ProductImageDTO>();
        public ICollection<ProductDiscountDTO> Discounts { get; set; } = new HashSet<ProductDiscountDTO>();
        public ProductDetailDTO? Detail { get; set; } = null;
        // Will add tags collection later when implementing tag functionality
        // public ICollection<TagDTO> Tags { get; set; } = new HashSet<TagDTO>();
    }
}