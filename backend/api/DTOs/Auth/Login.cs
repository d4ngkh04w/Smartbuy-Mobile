using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class Login
    {
        [Required]
        [Phone(ErrorMessage = "Invalid phone number")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    } 
}