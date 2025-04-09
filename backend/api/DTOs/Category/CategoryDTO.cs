using api.DTOs.Product;

namespace api.DTOs.Category
{
    public class CategoryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public HashSet<ProductDTO>? Products { get; set; } = null;
    }
}