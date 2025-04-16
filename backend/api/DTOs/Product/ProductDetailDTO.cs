namespace api.DTOs.Product
{
    public class ProductDetailDTO
    {
        public int Id { get; set; }
        public string Warranty { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string Processor { get; set; } = string.Empty;
        public string ScreenSize { get; set; } = string.Empty;
        public string ScreenResolution { get; set; } = string.Empty;
        public string Battery { get; set; } = string.Empty;
        public int SimSlots { get; set; } = 0;
    }
}