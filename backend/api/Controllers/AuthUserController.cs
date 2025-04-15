using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/user/auth")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AccountController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            var (success, errors) = await _authService.Register(register, "user");
            if (success) return Ok(new { Message = "User registered successfully" });

            return BadRequest(new { Message = "User registration failed", Errors = errors });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var result = await _authService.Login(login, "user");
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
                    result.token!.Token,
                    ExpireAt = DateTime.Now.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss"),
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
                    Message = message,
                    token!.Token,
                    ExpireAt = DateTime.Now.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }

            return Unauthorized(new { Message = message });
        }

        [Route("/api/v1/auth/refresh-token")]
        [HttpPost]
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
                    Expires = DateTimeOffset.Now.AddDays(7),
                });
                return Ok(new
                {
                    Message = "Token refreshed successfully",
                    result.Token!.Token,
                    ExpireAt = DateTime.Now.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss"),
                });
            }

            return Unauthorized(new { Message = result.ErrorMessage });
        }

        [Route("/api/v1/auth/logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { Message = "Refresh token is missing" });

            var result = await _tokenService.RevokeRefreshToken(refreshToken);
            if (result.Success)
            {
                Response.Cookies.Delete("refreshToken");
                return Ok(new { Message = "Logout successful" });
            }

            return Unauthorized(new { Message = result.ErrorMessage });
        }

        [Route("/api/v1/auth/verify")]
        [HttpGet]
        public IActionResult VerifyToken()
        {
            return Ok(new
            {
                Message = "Token is valid",
                IsAuthenticated = true
            });
        }
    }
}