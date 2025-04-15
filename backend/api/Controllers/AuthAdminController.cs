using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace api.Controllers.Auth.Admin
{
    [Route("api/admin/auth")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class AuthAdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthAdminController(IAuthService authService, ITokenService tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            var (success, errors) = await _authService.Register(register, "admin");
            if (success) return Ok(new { Message = "Admin registered successfully" });

            return BadRequest(new { Message = "Admin registration failed", Errors = errors });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            var result = await _authService.Login(login, "admin");
            if (result.Success)
                return Ok(new { Message = "Login successful", result.token!.Token, result.token!.RefreshToken });

            return Unauthorized(new { Message = result.ErrorMessage });
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLogin dto)
        {
            var (success, message, token) = await _authService.LoginWithGoogleAsync(dto, "admin");
            if (success) return Ok(new 
            { 
                Message = "Login successful", 
                Token = token!.Token,
                RefreshToken = token.RefreshToken
            });

            return Unauthorized(new { Message = message });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDTO refreshRequest)
        {
            var result = await _tokenService.ValidateRefreshToken(refreshRequest.RefreshToken);

            if (!result.Success || result.Token == null)
                return Unauthorized(new { Message = result.ErrorMessage });

            var claims = new JwtSecurityTokenHandler()
                .ReadJwtToken(result.Token.Token)
                .Claims;

            var roleClaim = claims.FirstOrDefault(c => c.Type == "role")?.Value;
            if (roleClaim != "admin")
                return Unauthorized(new { Message = "Invalid refresh token for admin" });

            return Ok(new
            {
                Message = "Token refreshed successfully",
                Token = result.Token.Token,
                RefreshToken = result.Token.RefreshToken
            });
        }

        [HttpGet("verify")]
        public IActionResult VerifyToken()
        {
            return Ok(new { Message = "Admin token is valid", IsAuthenticated = true });
        }
    }
}