using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/user/auth")]
    [ApiController]
    [Authorize]
    public class AuthUserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public AuthUserController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            var result = await _authService.Register(register, "user");
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
                    Message = "User registered successfully",
                });
            }

            return BadRequest(new { Message = "User registration failed", Errors = result.ErrorMessage });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var result = await _authService.Login(login, "user");
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

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLogin dto)
        {
            var (success, message, token) = await _authService.LoginWithGoogleAsync(dto, "user");
            if (success)
            {
                Response.Cookies.Append("token", token!.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddMinutes(int.Parse(_config["JWT:Expire"]!)),
                });
                Response.Cookies.Append("refreshToken", token!.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddDays(int.Parse(_config["JWT:RefreshTokenExpiry"]!)),
                });

                return Ok(new
                {
                    Message = message,
                });
            }

            return Unauthorized(new { Message = message });
        }

        [HttpGet("verify")]
        [Authorize(Roles = "user")]
        public IActionResult VerifyToken()
        {
            return Ok(new
            {
                Message = "Token is valid",
            });
        }
    }
}