using System.ComponentModel.DataAnnotations;
using api.Extensions;

namespace api.DTOs.User
{
    public class UpdateUserDTO
    {
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name can only contain letters and spaces")]
        public string? Name { get; set; }

        [RegularExpression(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$", ErrorMessage = "Email is not valid")]
        public string? Email { get; set; }

        [RegularExpression(@"^[\d]{10}$", ErrorMessage = "Phone number can only contain digits and spaces")]
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        [DataType(DataType.Upload)]
        [AllowFileExtension(extensions: [".jpg", ".jpeg", ".png"], ErrorMessage = "Logo must be a valid image (jpg, jpeg, png)")]
        public IFormFile? Avatar { get; set; }

        [RegularExpression(@"^(Nam|Nữ|Khác)$", ErrorMessage = "Gender must be male, female, or other")]
        public string? Gender { get; set; }
    }
}