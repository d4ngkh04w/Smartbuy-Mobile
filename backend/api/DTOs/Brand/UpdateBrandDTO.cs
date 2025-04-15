using System.ComponentModel.DataAnnotations;
using api.Extensions;

namespace api.DTOs.Brand
{
    public class UpdateBrandDTO
    {
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Brand name must be between 4 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Brand name can only contain letters and spaces")]
        public string? Name { get; set; } = null;

        [DataType(DataType.Upload)]
        [AllowFileExtension(extensions: [".jpg", ".jpeg", ".png"], ErrorMessage = "Logo must be a valid image (jpg, jpeg, png)")]
        public IFormFile? Logo { get; set; } = null;
    }
}