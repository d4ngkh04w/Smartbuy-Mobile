using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Tag
{
    public class CreateTagDTO
    {
        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Description must be less than 100 characters")]
        public string Description { get; set; } = string.Empty;
    }
}