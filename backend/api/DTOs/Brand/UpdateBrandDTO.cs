using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Brand
{
    public class UpdateBrandDTO
    {
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Brand name must be between 4 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Brand name can only contain letters and spaces")]
        public string? Name { get; set; } = null;

        [DataType(DataType.Upload)]
        [AllowFileExtension([".jpg", ".jpeg", ".png", ".svg", ".webp"])]
        public IFormFile? Logo { get; set; } = null;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }
    }
}