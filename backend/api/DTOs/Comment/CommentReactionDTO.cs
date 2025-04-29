using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Comment
{
    public class CommentReactionDTO
    {
        [Required]
        public bool IsLike { get; set; }
    }
}