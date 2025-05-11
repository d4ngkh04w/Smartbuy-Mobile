using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Auth
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Phone number is required")]
        [VietnamesePhoneNumber]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [Email]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}