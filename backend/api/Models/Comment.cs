using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("comments")]
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "text")]
        public string? Content { get; set; }
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;

        [Column(TypeName = "timestamp")]
        public DateTime? DeletedAt { get; set; } = null;
        public int? Rating { get; set; }
        public int? ParentId { get; set; } = null;

        [ForeignKey(nameof(ParentId))]
        public Comment? Parent { get; set; } = null;
        public int Likes { get; set; } = 0;
        public int Dislikes { get; set; } = 0;

        [Column(TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime UpdatedAt { get; set; }
    }
}