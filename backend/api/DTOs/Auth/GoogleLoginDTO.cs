using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class GoogleLoginDTO
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }
}