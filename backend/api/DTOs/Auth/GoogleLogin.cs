using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class GoogleLogin
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}