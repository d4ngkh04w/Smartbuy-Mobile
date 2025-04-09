using api.DTOs.Auth;
using api.Interfaces.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenService _tokenService;

        private readonly string _googleClientId;

        public AuthService(UserManager<IdentityUser> userManager, IConfiguration config)
        {
            _userManager = userManager;
            _tokenService = new TokenService(config);
            _googleClientId = config["Google:ClientId"]!;
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

        public async Task<(bool success, string? token, string message)> LoginWithGoogleAsync(GoogleLogin dto, string role)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _googleClientId },
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token, settings);
                if (payload == null)
                {
                    return (false, null, "Token không hợp lệ");
                }
                var user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new IdentityUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        PhoneNumber = null
                    };

                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        return (false, null, "Không thể tạo tài khoản người dùng");
                    }

                    await _userManager.AddToRoleAsync(user, role);
                }

                var token = _tokenService.CreateToken(user, role);
                return (true, token, "Đăng nhập bằng Google thành công");
            }
            catch (Exception ex)
            {
                return (false, null, $"Lỗi: {ex.Message}");
            }
        }
    }
}