using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Product
{
    public class UpdateProductDTO
    {
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        [RegularExpression(@"^[\p{L}\p{N}\s\-\/\(\)]+$", ErrorMessage = "Name can only contain letters, numbers, spaces, hyphens, slashes, and parentheses")]
        public string? Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "Import price must be greater than 0")]
        public decimal? ImportPrice { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "Sale price must be greater than 0")]
        public decimal? SalePrice { get; set; }
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ProductLine ID must be greater than 0")]
        public int? ProductLineId { get; set; }
        public string? Warranty { get; set; }
        public string? RAM { get; set; }
        public string? Storage { get; set; }
        public string? Processor { get; set; }
        public string? ScreenSize { get; set; }
        public string? ScreenResolution { get; set; }
        public string? Battery { get; set; }
        public int? SimSlots { get; set; }
        public string? OS { get; set; }
    }
}