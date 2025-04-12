using api.DTOs.Auth;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Models;
using Google.Apis.Auth;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly string _googleClientId;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IConfiguration config)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _googleClientId = config["Google:ClientId"]!;
        }

        public async Task<(bool Success, string? ErrorMessage)> Register(Register registerDto, string role)
        {
            if (await _userRepository.UserExistsByPhoneNumberAsync(registerDto.PhoneNumber))
                return (false, "Phone number already exists");
            if (await _userRepository.UserExistsByEmailAsync(registerDto.Email))
                return (false, "Email already exists");

            try
            {
                var user = new User
                {
                    PhoneNumber = registerDto.PhoneNumber,
                    Email = registerDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = role
                };

                await _userRepository.CreateUserAsync(user);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Login(Login loginDto, string role)
        {
            var user = await _userRepository.GetUserByPhoneNumberAsync(loginDto.PhoneNumber);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return (false, "Invalid phone number or password", null);
            }
            if (user.Role != role)
            {
                return (false, $"Your account does not have access as a '{role}'", null);
            }
            user.LastLogin = DateTime.Now;
            await _userRepository.UpdateUserAsync(user);
            var token = _tokenService.CreateToken(user, role);
            var refreshToken = await _tokenService.GenerateRefreshToken(user);
            return (true, "Login successful", new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> LoginWithGoogleAsync(GoogleLogin dto, string role)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _googleClientId },
                });

                if (payload == null)
                {
                    return (false, "Invalid Google token", null);
                }

                if (payload.Email != dto.Email)
                {
                    return (false, "Email from token doesn't match provided email", null);
                }

                var user = await _userRepository.GetUserByEmailAsync(payload.Email);
                if (user == null)
                {
                    user = new User
                    {
                        PhoneNumber = string.Empty,
                        Email = payload.Email,
                        Name = payload.Name ?? string.Empty,
                        Password = string.Empty,
                        Role = role,
                        EmailConfirmed = true
                    };

                    await _userRepository.CreateUserAsync(user);
                }
                else if (user.Role != role)
                {
                    return (false, $"Your account does not have access as a '{role}'", null);
                }

                var token = _tokenService.CreateToken(user, role);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);
                user.LastLogin = DateTime.Now;
                await _userRepository.UpdateUserAsync(user);
                return (true, "Login with Google successful", new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }
    }
}