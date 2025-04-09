using System.ComponentModel.DataAnnotations;
using api.Extensions;

namespace api.DTOs.Brand
{
    public class CreateBrandDTO
    {
        [Required(ErrorMessage = "Brand name is required")]
        [RegularExpression(@"^[a-zA-Z\s]{4,100}$", ErrorMessage = "Brand name must be between 4 and 100 characters and can only contain letters and spaces")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand logo is required")]
        [DataType(DataType.Upload)]
        [AllowExtension(Extensions: [".jpg", ".jpeg", ".png"], ErrorMessage = "Logo must be a valid image (jpg, jpeg, png)")]
        public IFormFile Logo { get; set; } = null!;
    }
}