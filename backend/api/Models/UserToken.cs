using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Table("user_tokens")]
    [Index(nameof(TokenHash))]
    public class UserToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string TokenHash { get; set; } = string.Empty;

        [Column(TypeName = "enum('RefreshToken', 'PasswordResetToken', 'EmailVerificationToken')")]
        public string TokenType { get; set; } = string.Empty;

        [Column(TypeName = "timestamp")]
        public DateTime ExpiryDate { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsUsed { get; set; } = false;

        public bool IsRevoked { get; set; } = false;

        [Column(TypeName = "timestamp")]
        public DateTime? RevokedAt { get; set; } = null;
    }
}