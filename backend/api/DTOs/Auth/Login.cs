using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class Login
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        // [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9])(?=.*[!@#$%^&*])[a-zA-Z0-9!@#$%^&*]{6,}$", ErrorMessage = "Password must contain at least one letter, one number, and one special character")]
        public string Password { get; set; } = string.Empty;
    }
}