using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class UserRole
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}