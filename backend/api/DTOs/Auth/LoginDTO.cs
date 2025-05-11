using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.Auth
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Phone number is required")]
        [VietnamesePhoneNumber]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}