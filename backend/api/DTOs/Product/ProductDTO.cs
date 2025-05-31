using api.DTOs.Discount;

namespace api.DTOs.Product
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Price { get; set; }
        public string Discount { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public int Sold { get; set; }
        public bool IsActive { get; set; } = true;
        public bool ManuallyDeactivated { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int ProductLineId { get; set; }
        public string ProductLineName { get; set; } = string.Empty;
        public ICollection<ProductColorDTO> Colors { get; set; } = new HashSet<ProductColorDTO>();
        public ICollection<DiscountDTO> Discounts { get; set; } = new HashSet<DiscountDTO>();
        public ProductDetailDTO? Detail { get; set; } = null;
        // Will add tags collection later when implementing tag functionality
        // public ICollection<TagDTO> Tags { get; set; } = new HashSet<TagDTO>();
    }
}