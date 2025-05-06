using System.Security.Claims;
using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/user/auth")]
    [ApiController]
    [Authorize(Roles = "user")]
    public class AuthUserController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public AuthUserController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        private string GetCurrentUserEmail()
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (emailClaim == null)
                return string.Empty;

            return emailClaim;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDTO register)
        {
            var result = await _authService.Register(register, "user");
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
                Expires = DateTimeOffset.Now.AddMinutes(double.Parse(_config["JWT:Expire"]!)),
            });

            Response.Cookies.Append("refreshToken", result.token!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(double.Parse(_config["JWT:RefreshTokenExpiry"]!)),
            });

            return Ok(new
            {
                Message = "User registered successfully",
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var result = await _authService.Login(login, "user");
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Does not have access", StringComparison.OrdinalIgnoreCase) => Forbid(),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => Unauthorized(new { Message = result.ErrorMessage })
                };
            }

            Response.Cookies.Append("token", result.token!.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddMinutes(double.Parse(_config["JWT:Expire"]!)),
            });
            Response.Cookies.Append("refreshToken", result.token!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(double.Parse(_config["JWT:RefreshTokenExpiry"]!)),
            });

            return Ok(new
            {
                Message = "Login successful",
            });
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO dto)
        {
            var result = await _authService.LoginWithGoogleAsync(dto, "user");
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => Unauthorized(new { Message = result.ErrorMessage })
                };
            }

            Response.Cookies.Append("token", result.token!.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddMinutes(double.Parse(_config["JWT:Expire"]!)),
            });
            Response.Cookies.Append("refreshToken", result.token!.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = DateTimeOffset.Now.AddDays(double.Parse(_config["JWT:RefreshTokenExpiry"]!)),
            });

            return Ok(new
            {
                Message = "Login successful",
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

        [HttpPost("send-verification-email")]
        public async Task<IActionResult> SendVerificationEmail()
        {
            var email = GetCurrentUserEmail();
            var result = await _authService.SendEmailVerificationAsync(email);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new { Message = result.ErrorMessage });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDTO verifyDto)
        {
            var result = await _authService.VerifyEmailAsync(verifyDto);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.ErrorMessage ?? "Email verification failed" });
            }

            return Ok(new { Message = "Email verified successfully" });
        }
    }
}