using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.ProductLine
{
    public class UpdateProductLineDTO
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product line name must be between 2 and 100 characters")]
        [RegularExpression(@"^[\p{L}0-9\s]+$", ErrorMessage = "Product line name can only contain letters, numbers, and spaces")]
        public string? Name { get; set; }

        [StringLength(2000, ErrorMessage = "Description must be less than 2000 characters")]
        public string? Description { get; set; }

        [DataType(DataType.Upload)]
        [AllowFileExtension([".jpg", ".jpeg", ".png", ".svg", ".webp"])]
        public IFormFile? Image { get; set; } = null;

        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be a positive integer")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Brand ID must be a positive integer")]
        public int? BrandId { get; set; }
    }
}