using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Product
{
    public class UpdateProductDTO
    {
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Name can only contain letters, numbers, and spaces")]
        public string? Name { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int? Quantity { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "Import price must be greater than 0")]
        public decimal? ImportPrice { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "Sale price must be greater than 0")]
        public decimal? SalePrice { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }

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
        
        // Updated to support color-image relationship
        public List<UpdateColorDTO>? UpdateColorData { get; set; }
        public List<int>? RemoveColorIds { get; set; }
    }

    public class UpdateColorDTO
    {
        public int? Id { get; set; }  // If null, creates a new color
        
        [StringLength(50, ErrorMessage = "Color name must be less than 50 characters")]
        public string? Name { get; set; }
        
        // Images to add to this color
        public List<IFormFile>? AddImages { get; set; }
        
        // Images to remove from this color
        public List<int>? RemoveImageIds { get; set; }
        
        // Setting main image
        public int? MainImageId { get; set; }  // ID of an existing image to set as main
        public int MainImageIndex { get; set; } = 0; // Index in AddImages to set as main
        public bool SetMainImage { get; set; } = false; // Whether to set a main image
    }
}