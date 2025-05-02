using System.ComponentModel.DataAnnotations;
using api.Extensions;

namespace api.DTOs.Product
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\/]+$", ErrorMessage = "Name can only contain letters, numbers, spaces, hyphens, and slashes")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Import price is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Import price must be greater than 0")]
        public decimal ImportPrice { get; set; }

        [Required(ErrorMessage = "Sale price is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Sale price must be greater than 0")]
        public decimal SalePrice { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be less than 2000 characters")]
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be greater than 0")]
        public int BrandId { get; set; }

        [Required(ErrorMessage = "ProductLine ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "ProductLine ID must be greater than 0")]
        public int ProductLineId { get; set; }

        // Thông số kỹ thuật không còn là bắt buộc
        [Range(1, 60, ErrorMessage = "Warranty must be between 1 and 60 months")]
        public int Warranty { get; set; } = 12;

        [Range(1, 32, ErrorMessage = "RAM must be between 1 and 32 GB")]
        public int? RAM { get; set; }

        [Range(8, 2048, ErrorMessage = "Storage must be between 8 and 2048 GB")]
        public int? Storage { get; set; }

        [Range(3.0, 15.0, ErrorMessage = "Screen size must be between 3.0 and 15.0 inches")]
        public decimal? ScreenSize { get; set; }

        [RegularExpression(@"^\d{3,4}x\d{3,4}$", ErrorMessage = "Screen resolution must be in the format '1920x1080'")]
        public string? ScreenResolution { get; set; } = string.Empty;

        [Range(1000, 10000, ErrorMessage = "Battery must be between 1000 and 10000 mAh")]
        public int? Battery { get; set; }

        [StringLength(50, ErrorMessage = "Operating system must be less than 50 characters")]
        public string? OS { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Processor must be less than 100 characters")]
        public string? Processor { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Sim slots must be between 1 and 4")]
        public int SimSlots { get; set; } = 1;

        public List<int>? TagIds { get; set; }

        // Màu sắc và hình ảnh không còn là bắt buộc
        public List<ColorDataDTO>? ColorData { get; set; } = new List<ColorDataDTO>();
    }

    public class ColorDataDTO
    {
        [Required(ErrorMessage = "Color name is required")]
        [StringLength(50, ErrorMessage = "Color name must be less than 50 characters")]
        public string Name { get; set; } = string.Empty;

        [AllowFileExtension(extensions: new string[] {".jpg", ".jpeg", ".png"}, ErrorMessage = "Images must be valid images (jpg, jpeg, png)")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        public int MainImageIndex { get; set; } = 0;
    }
}