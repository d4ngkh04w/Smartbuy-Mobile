using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Table("users")]
    [Index(nameof(Email), nameof(PhoneNumber), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(60)")]
        public string Password { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }

        [Column(TypeName = "varchar(10)")]
        public string PhoneNumber { get; set; } = string.Empty;
        public bool PhoneNumberConfirmed { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "enum('Nam', 'Nữ', 'Khác')")]
        public string? Gender { get; set; }

        [Column(TypeName = "varchar(70)")]
        public string? Avatar { get; set; } = null;

        [Column(TypeName = "enum('user', 'admin')")]
        public string Role { get; set; } = string.Empty;

        [Column(TypeName = "timestamp")]
        public DateTime CreatedAt { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? LastLogin { get; set; }
        public string? RefreshToken { get; set; } = null;

        [Column(TypeName = "timestamp")]
        public DateTime? RefreshTokenExpiry { get; set; } = null;

        public string? PasswordResetToken { get; set; } = null;

        [Column(TypeName = "timestamp")]
        public DateTime? PasswordResetTokenExpiry { get; set; } = null;
    }
}