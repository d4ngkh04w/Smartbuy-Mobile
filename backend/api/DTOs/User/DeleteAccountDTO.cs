using System.ComponentModel.DataAnnotations;

namespace api.DTOs.User
{
    public class DeleteAccountDTO
    {
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
