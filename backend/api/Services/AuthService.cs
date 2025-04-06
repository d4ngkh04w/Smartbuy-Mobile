using api.DTOs.Auth;
using api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;

        public AuthService(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _tokenService = new TokenService(config);
        }

        public async Task<(bool success, string? errors)> Register(Register registerDto, string role)
        {
            if (registerDto.Password != registerDto.ConfirmPassword) return (false, "Passwords do not match");
            if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == registerDto.PhoneNumber))
                return (false, "Phone number already exists");
            if (await _userManager.Users.AnyAsync(u => u.Email == registerDto.Email))
                return (false, "Email already exists");

            var user = new IdentityUser
            {
                UserName = registerDto.PhoneNumber,
                PhoneNumber = registerDto.PhoneNumber,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password)
            };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return (true, null);
            }

            return (false, result.Errors.Select(e => e.Description).FirstOrDefault());
        }

        public async Task<(bool success, string? token)> Login(Login loginDto, string role)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginDto.PhoneNumber);
            if (user != null && BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return (true, _tokenService.CreateToken(user, role));
            }
            return (false, null);
        }
    }
}