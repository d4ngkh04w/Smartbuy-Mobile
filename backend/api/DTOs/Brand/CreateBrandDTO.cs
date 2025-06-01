using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Brand
{
    public class CreateBrandDTO
    {
        [Required(ErrorMessage = "Brand name is required")]
        [RegularExpression(@"^[a-zA-Z\s]{4,100}$", ErrorMessage = "Brand name must be between 4 and 100 characters and can only contain letters and spaces")]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.Upload)]
        [AllowFileExtension([".jpg", ".jpeg", ".png", ".svg", ".webp"])]
        public IFormFile? Logo { get; set; } = null;

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        public bool? IsActive { get; set; } = true;
    }
}