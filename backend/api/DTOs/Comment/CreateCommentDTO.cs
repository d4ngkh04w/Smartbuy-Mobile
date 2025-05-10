using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Comment
{
    public class CreateCommentDTO
    {
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string? Content { get; set; }

        [Required]
        public int ProductId { get; set; }
        public int? Rating { get; set; }

        public int? ParentId { get; set; }
    }
}