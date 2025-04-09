using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Category
{
    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Category name must be between 5 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Category name can only contain letters, numbers, and spaces")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be a positive integer")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Brand ID must be a positive integer")]
        public int BrandId { get; set; }
    }
}