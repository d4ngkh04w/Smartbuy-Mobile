using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Auth
{
    public class VerifyEmailDTO
    {
        [Required]
        [Email]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
