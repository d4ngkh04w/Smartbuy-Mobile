using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Tag
{
    public class UpdateTagDTO
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Tag name must be between 2 and 50 characters")]
        public string? Name { get; set; } = string.Empty;
    }
}