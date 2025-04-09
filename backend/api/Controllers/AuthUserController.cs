using api.DTOs.Auth;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Auth.User
{
    [Route("api/user/auth")]
    [ApiController]    
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            if (!ModelState.IsValid) return BadRequest(new { Message = "Invalid data" });

            var (success, errors) = await _authService.Register(register, "user");
            if (success) return Ok(new { Message = "User registered successfully" });
            return BadRequest(new { Message = "User registration failed", Errors = errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (!ModelState.IsValid) return BadRequest(new { Message = "Invalid data" });

            var (success, token) = await _authService.Login(login, "user");
            if (success) return Ok(new { token, phoneNumber = login.PhoneNumber });
            return Unauthorized(new { Message = "Invalid phone number or password" });
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLogin dto)
        {   
            if (!ModelState.IsValid) return BadRequest(new { Message = "Invalid data" });

            var (success, token, message) = await _authService.LoginWithGoogleAsync(dto, "user");
            if (success) return Ok(new { token, message });
            return Unauthorized(new { message });
        }
    }
}