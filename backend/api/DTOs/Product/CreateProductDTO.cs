using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Product
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
       [RegularExpression(@"^[\p{L}\p{N}\s\-\/\(\)]+$", ErrorMessage = "Name can only contain letters, numbers, spaces, hyphens, slashes, and parentheses")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Import price is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Import price must be greater than 0")]
        public decimal ImportPrice { get; set; }

        [Required(ErrorMessage = "Sale price is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Sale price must be greater than 0")]
        public decimal SalePrice { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be less than 2000 characters")]
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "ProductLine ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductLine ID must be greater than 0")]
        public int ProductLineId { get; set; }

        [Range(1, 60, ErrorMessage = "Warranty must be between 1 and 60 months")]
        public int? Warranty { get; set; }

        [Range(1, 32, ErrorMessage = "RAM must be between 1 and 32 GB")]
        public int? RAM { get; set; }

        [Range(8, 2048, ErrorMessage = "Storage must be between 8 and 2048 GB")]
        public int? Storage { get; set; }

        [Range(3.0, 15.0, ErrorMessage = "Screen size must be between 3.0 and 15.0 inches")]
        public decimal? ScreenSize { get; set; }

        [RegularExpression(@"^\d{3,4}x\d{3,4}$", ErrorMessage = "Screen resolution must be in the format '1920x1080'")]
        public string? ScreenResolution { get; set; }

        [Range(1000, 10000, ErrorMessage = "Battery must be between 1000 and 10000 mAh")]
        public int? Battery { get; set; }

        [StringLength(50, ErrorMessage = "Operating system must be less than 50 characters")]
        public string? OS { get; set; }

        [StringLength(100, ErrorMessage = "Processor must be less than 100 characters")]
        public string? Processor { get; set; }

        [Range(1, 4, ErrorMessage = "Sim slots must be between 1 and 4")]
        public int? SimSlots { get; set; }

        // public List<int>? TagIds { get; set; }
    }
}