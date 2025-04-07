using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class Register
    {
        [Required]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}