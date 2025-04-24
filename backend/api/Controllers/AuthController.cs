using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, ITokenService tokenService, IConfiguration config)
        {
            _authService = authService;
            _tokenService = tokenService;
            _config = config;
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { Message = "Refresh token is missing" });

            var result = await _tokenService.ValidateRefreshToken(refreshToken);
            if (result.Success)
            {
                Response.Cookies.Append("refreshToken", result.Token!.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddDays(int.Parse(_config["JWT:RefreshTokenExpiry"]!)),
                });
                Response.Cookies.Append("token", result.Token!.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = DateTimeOffset.Now.AddMinutes(int.Parse(_config["JWT:Expire"]!)),
                });
                return Ok(new
                {
                    Message = "Token refreshed successfully",
                });
            }

            return Unauthorized(new { Message = result.ErrorMessage });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var token = Request.Cookies["token"];
            if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(token))
                return Unauthorized(new { Message = "Token is missing" });

            var result = await _tokenService.RevokeRefreshToken(refreshToken);
            if (result.Success)
            {
                Response.Cookies.Delete("refreshToken");
                Response.Cookies.Delete("token");
                return Ok(new { Message = "Logged out successfully" });
            }

            return BadRequest(new { Message = result.ErrorMessage });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
        {
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);

            if (!result.Success && result.ErrorMessage != null && result.ErrorMessage.Contains("Failed to send"))
            {
                return StatusCode(500, new { Message = "Failed to send password reset email. Please try again later." });
            }

            return Ok(new { Message = "If your email exists in our system, you will receive password reset instructions." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (!result.Success)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new { Message = "Password has been reset successfully. You can now log in with your new password." });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "User not authenticated" });

            var result = await _authService.ChangePasswordAsync(changePasswordDto, Guid.Parse(userId));

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("User not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Incorrect", StringComparison.OrdinalIgnoreCase) => BadRequest(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new { Message = "Password changed successfully" });
        }
    }
}