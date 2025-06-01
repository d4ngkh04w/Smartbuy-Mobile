using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Product
{
    public class CreateColorDTO
    {
        [Required(ErrorMessage = "Color name is required")]
        public string Name { get; set; } = string.Empty;
        
        [AllowFileExtension([".jpg", ".jpeg", ".png"], ErrorMessage = "Only .jpg, .jpeg, and .png files are allowed")]
        public List<IFormFile>? Images { get; set; } = new List<IFormFile>();
        public int? MainImageIndex { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int Quantity { get; set; } = 0;
    }
}