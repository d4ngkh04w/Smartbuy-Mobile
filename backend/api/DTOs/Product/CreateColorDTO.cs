using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Product
{
    public class CreateColorDTO
    {
        [Required(ErrorMessage = "Color name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "At least one image is required")]
        [AllowFileExtension([".jpg", ".jpeg", ".png"], ErrorMessage = "Only .jpg, .jpeg, and .png files are allowed")]
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public int? MainImageIndex { get; set; } = 0;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; } = 0;
    }
}