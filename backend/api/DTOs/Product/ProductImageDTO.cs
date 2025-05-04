namespace api.DTOs.Product
{
    public class ProductImageDTO
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public bool IsMain { get; set; }
    }
}