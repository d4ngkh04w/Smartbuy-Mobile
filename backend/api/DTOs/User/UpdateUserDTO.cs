using System.ComponentModel.DataAnnotations;
using api.Annotations;

namespace api.DTOs.User
{
    public class UpdateUserDTO
    {
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string? Name { get; set; }

        [Email]
        public string? Email { get; set; }

        [VietnamesePhoneNumber]
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        [AllowFileExtension([".jpg", ".jpeg", ".png", ".webp"])]
        public IFormFile? Avatar { get; set; }

        [RegularExpression(@"^(Nam|Nữ|Khác)$", ErrorMessage = "Gender must be male, female, or other")]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }
    }
}