using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.ProductLine
{
    public class CreateProductLineDTO
    {
        [Required(ErrorMessage = "Product line name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product line name must be between 2 and 100 characters")]
        [RegularExpression(@"^[\p{L}0-9\s]+$", ErrorMessage = "Product line name can only contain letters, numbers, and spaces")]

        public string Name { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Description must be less than 2000 characters")]
        public string? Description { get; set; }

        [AllowFileExtension([".jpg", ".jpeg", ".png", ".svg", ".webp"])]
        public IFormFile? Image { get; set; } = null;

        [Required(ErrorMessage = "Brand ID is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Brand ID must be a positive integer")]
        public int BrandId { get; set; }
    }
}