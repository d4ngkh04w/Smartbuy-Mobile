using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class Login
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    } 
}