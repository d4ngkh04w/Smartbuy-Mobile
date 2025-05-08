using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Auth
{
    public class ForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email is required")]
        [Email]
        public string Email { get; set; } = string.Empty;
    }
}