using api.DTOs.Product;

namespace api.DTOs.ProductLine
{
    public class ProductLineDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool ManuallyDeactivated { get; set; } = false;
        public string BrandName { get; set; } = string.Empty;
        public HashSet<ProductDTO> Products { get; set; } = new HashSet<ProductDTO>();
    }
}