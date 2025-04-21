using api.DTOs.ProductLine;

namespace api.DTOs.Brand
{
    public class BrandDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public ICollection<ProductLineDTO>? ProductLines { get; set; } = null!;
    }
}