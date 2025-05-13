namespace api.DTOs.Product
{
    public class ProductColorDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public ICollection<ProductImageDTO> Images { get; set; } = new HashSet<ProductImageDTO>();
    }
}