using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Comment
{
    public class UpdateCommentDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;

        public int? Rating { get; set; }
    }
}