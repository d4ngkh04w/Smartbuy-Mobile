using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Product
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Name can only contain letters, numbers, and spaces")]
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
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        public int CategoryId { get; set; }

        [RegularExpression(@"^\d+ [a-zA-ZÀ-ỹ]+", ErrorMessage = "Warranty must be in the format '1 year'")]
        public string Warranty { get; set; } = string.Empty;

        [RegularExpression(@"^\d{1,3} GB$", ErrorMessage = "Storage must be in the format '512 GB'")]
        public string RAM { get; set; } = string.Empty;

        [RegularExpression(@"^\d{1,3} GB$", ErrorMessage = "Storage must be in the format '512 GB'")]
        public string Storage { get; set; } = string.Empty;

        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Processor can only contain letters, numbers, and spaces")]
        public string Processor { get; set; } = string.Empty;

        [RegularExpression(@"^\d+(\.\d+)? inch$", ErrorMessage = "Screen size must be in the format '6.7 inch'")]
        public string ScreenSize { get; set; } = string.Empty;

        [RegularExpression(@"^\d{3,4} x \d{3,4}$", ErrorMessage = "Screen resolution must be in the format '1920 x 1080'")]
        public string ScreenResolution { get; set; } = string.Empty;

        [RegularExpression(@"^\d{4} mAh$", ErrorMessage = "Battery must be in the format '5000 mAh'")]
        public string Battery { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Sim slots must be between 1 and 4")]
        public int SimSlots { get; set; } = 0;
        public ICollection<string>? Colors { get; set; }

        [Required(ErrorMessage = "At least one image is required")]
        [DataType(DataType.Upload)]
        public ICollection<IFormFile> Images { get; set; } = new HashSet<IFormFile>();
    }
}