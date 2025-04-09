using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Auth
{
    public class GoogleLogin
    {
       [Required]
       public string Token { get; set; }  = string.Empty;// Đây là credential từ Google gửi về frontend
    } 
}