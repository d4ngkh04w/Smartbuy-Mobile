using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class VerifyEmailDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
