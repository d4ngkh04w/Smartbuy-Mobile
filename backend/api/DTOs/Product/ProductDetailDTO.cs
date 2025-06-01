namespace api.DTOs.Product
{
    public class ProductDetailDTO
    {
        public string? Warranty { get; set; }
        public string? RAM { get; set; }
        public string? Storage { get; set; }
        public string? Processor { get; set; }
        public string? OperatingSystem { get; set; }
        public string? ScreenSize { get; set; }
        public string? ScreenResolution { get; set; }
        public string? Battery { get; set; }
        public int? SimSlots { get; set; }
    }
}