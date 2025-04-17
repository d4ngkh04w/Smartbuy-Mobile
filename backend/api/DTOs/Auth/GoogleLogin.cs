using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class GoogleLogin
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty; // Đây là credential từ Google gửi về frontend

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email")]
        public string Email { get; set; } = string.Empty; // Đây là email của người dùng, được lấy từ token
    }
}