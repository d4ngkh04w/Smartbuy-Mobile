using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Product
{
    public class UpdateColorDTO
    {
        [StringLength(50, ErrorMessage = "Color name must be less than 50 characters")]
        public string? Name { get; set; }

        [AllowFileExtension([".jpg", ".jpeg", ".png", ".svg", ".webp"])]
        public List<IFormFile>? AddImages { get; set; }
        public int? Quantity { get; set; }
        public List<int>? RemoveImageIds { get; set; }
        public int? MainImageIndex { get; set; }
        public int? MainImageId { get; set; }
    }
}