using api.DTOs.Auth;
using api.Exceptions;
using api.Helpers;
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

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = CookieHelper.RefreshToken;
            if (string.IsNullOrEmpty(refreshToken))
                throw new UnauthorizedException("Refresh token is missing");

            var token = await _tokenService.ValidateRefreshToken(refreshToken);
            CookieHelper.AccessToken = token.AccessToken;
            CookieHelper.RefreshToken = token.RefreshToken;

            return ApiResponseHelper.Success<object>("Token refreshed successfully", null);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = CookieHelper.RefreshToken;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RevokeRefreshToken(refreshToken);
            }

            CookieHelper.RemoveAuthTokens();

            return ApiResponseHelper.Success<object>("Logged out successfully", null);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDto)
        {
            await _authService.ForgotPasswordAsync(forgotPasswordDto);
            return ApiResponseHelper.Success<object>("If the email address exists in our system, we will send a password reset link", null);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDto)
        {
            await _authService.ResetPasswordAsync(resetPasswordDto);
            return ApiResponseHelper.Success<object>("Password has been reset successfully. You can now log in with your new password", null);
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDto)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            await _authService.ChangePasswordAsync(changePasswordDto, userId);

            return ApiResponseHelper.Success<object>("Password changed successfully", null);
        }
    }
}