using api.DTOs.Auth;
using api.Interfaces;
using api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;

        public AccountController(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _tokenService = new TokenService(config);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Register register)
        {
            if (!ModelState.IsValid) return BadRequest(new { Message = "Invalid data" });
            if (register.Password != register.ConfirmPassword) return BadRequest(new { Message = "Passwords do not match" });

            var user = new IdentityUser
            {
                UserName = register.Username,
                Email = register.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password)
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                return Ok(new { Message = "User registered successfully" });
            }

            return BadRequest(new { Message = "User registration failed", Errors = result.Errors });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (!ModelState.IsValid) return BadRequest(new { Message = "Invalid data" });

            var user = await _userManager.FindByNameAsync(login.Username);
            if (user != null && BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
            {
                return Ok(new { token = _tokenService.CreateToken(user, "user"), username = user.UserName });
            }
            return Unauthorized(new { Message = "Invalid username or password" });
        }
    }
}