using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/admin/auth")]
    [ApiController]
    [Authorize]
    public class AuthAdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;
        public AuthAdminController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            var result = await _authService.Register(register, "admin");
            if (result.Success)
            {
                Response.Cookies.Append("token", result.token!.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddMinutes(int.Parse(_config["JWT:Expire"]!)),
                });
                Response.Cookies.Append("refreshToken", result.token!.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddDays(int.Parse(_config["JWT:RefreshTokenExpiry"]!)),
                });
                return Ok(new
                {
                    Message = "Admin registered successfully",
                });
            }

            return BadRequest(new { Message = "Admin registration failed", Errors = result.ErrorMessage });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var result = await _authService.Login(login, "admin");
            if (result.Success)
            {
                Response.Cookies.Append("token", result.token!.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddMinutes(int.Parse(_config["JWT:Expire"]!)),
                });
                Response.Cookies.Append("refreshToken", result.token!.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddDays(int.Parse(_config["JWT:RefreshTokenExpiry"]!)),
                });
                return Ok(new
                {
                    Message = "Login successful",
                });
            }

            return Unauthorized(new { Message = result.ErrorMessage });
        }

        [HttpGet("verify")]
        [Authorize(Roles = "admin")]
        public IActionResult VerifyToken()
        {
            return Ok(new
            {
                Message = "Token is valid",
            });
        }

    }
}