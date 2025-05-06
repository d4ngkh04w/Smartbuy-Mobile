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
        private readonly IConfiguration _config;
        public AuthAdminController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO register)
        {
            var result = await _authService.Register(register, "admin");
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Already exists", StringComparison.OrdinalIgnoreCase) => Conflict(new { Message = result.ErrorMessage }),
                    _ => StatusCode(500, new { Message = result.ErrorMessage })
                };
            }

            Response.Cookies.Append("token", result.token!.AccessToken, new CookieOptions
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

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var result = await _authService.Login(login, "admin");
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Does not have access", StringComparison.OrdinalIgnoreCase) => Forbid(),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            Response.Cookies.Append("token", result.token!.AccessToken, new CookieOptions
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
                Message = "Admin logged in successfully",
            });
        }

        [HttpGet("verify")]
        public IActionResult VerifyToken()
        {
            return Ok(new
            {
                Message = "Token is valid",
            });
        }

    }
}