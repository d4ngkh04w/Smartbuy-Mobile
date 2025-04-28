using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Comment
{
    public class CommentDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatar { get; set; }
        public int ProductId { get; set; }
        public int? Rating { get; set; }
        public int? ParentId { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public List<CommentDTO>? Replies { get; set; }
    }

    public class UpdateCommentDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "Content cannot be empty")]
        [MaxLength(1000, ErrorMessage = "Content cannot exceed 1000 characters")]
        public string Content { get; set; } = string.Empty;

        public int? Rating { get; set; }
    }

    public class CommentReactionDTO
    {
        [Required]
        public bool IsLike { get; set; }
    }

    public class CommentResponseDTO
    {
        public List<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}