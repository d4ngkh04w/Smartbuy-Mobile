namespace api.DTOs.Product
{
    public class ProductColorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<ProductImageDTO> Images { get; set; } = new HashSet<ProductImageDTO>();
        public bool HasMainImage { get; set; } = false;
    }
}