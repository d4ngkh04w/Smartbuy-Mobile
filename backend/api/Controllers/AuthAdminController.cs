using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/admin/auth")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AuthAdminController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthAdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            var result = await _authService.Register(register, "admin");
            if (result.Success) return Ok(new
            {
                Message = "Admin registered successfully",
                result.token!.Token,
                ExpireAt = DateTime.Now.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss"),
            });

            return BadRequest(new { Message = "Admin registration failed", Errors = result.ErrorMessage });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var result = await _authService.Login(login, "admin");

            if (result.Success)
            {
                Response.Cookies.Append("refreshToken", result.token!.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddDays(7),
                });
                return Ok(new
                {
                    Message = "Login successful",
                    Token = result.token!.Token,
                    ExpireAt = DateTime.Now.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }

            return Unauthorized(new { Message = result.ErrorMessage });
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLogin dto)
        {
            var (success, message, token) = await _authService.LoginWithGoogleAsync(dto, "admin");
            if (success)
            {
                Response.Cookies.Append("refreshToken", token!.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddDays(7),
                });
                return Ok(new
                {
                    Message = "Login successful",
                    token!.Token,
                    ExpireAt = DateTime.Now.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }

            return Unauthorized(new { Message = message });
        }
    }
}