using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Category
{
    public class UpdateCategoryDTO
    {
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Category name must be between 4 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s]+$", ErrorMessage = "Category name can only contain letters, numbers, and spaces")]
        public string? Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Brand ID must be a positive integer")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Brand ID must be a positive integer")]
        public int? BrandId { get; set; }
    }
}