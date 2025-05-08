using api.DTOs.Auth;
using api.Helpers;
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
        public async Task<IActionResult> Register([FromBody] RegisterDTO register)
        {
            var token = await _authService.Register(register, "admin");
            CookieHelper.AccessToken = token.AccessToken;
            CookieHelper.RefreshToken = token.RefreshToken;

            return ApiResponseHelper.Success<object>("Admin registered successfully", null);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO login)
        {
            var token = await _authService.Login(login, "admin");
            CookieHelper.AccessToken = token.AccessToken;
            CookieHelper.RefreshToken = token.RefreshToken;
            return ApiResponseHelper.Success<object>("Login successful", null);
        }

        [HttpGet("verify")]
        public IActionResult VerifyToken()
        {
            return Ok(new
            {
                Message = "Token is valid",
                UserId = HttpContextHelper.CurrentUserId,
                Email = HttpContextHelper.CurrentUserEmail,
                Role = HttpContextHelper.CurrentUserRole
            });
        }

    }
}