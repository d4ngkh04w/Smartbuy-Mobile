using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using api.DTOs.Auth;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Models;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        private readonly IUserRepository _userRepository;

        public TokenService(IUserRepository userRepository, IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
            _userRepository = userRepository;
        }

        public string CreateToken(User user, string role)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("phone", user.PhoneNumber),
                new Claim("role", role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(_key, _config["JWT:Algorithm"]!);

            var tokenDescriptor = new JwtSecurityToken(
                audience: _config["JWT:Audience"],
                issuer: _config["JWT:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_config["JWT:Expire"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(int.Parse(_config["JWT:RefreshTokenExpiry"]!));
            await _userRepository.UpdateUserAsync(user);
            return refreshToken;
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? Token)> ValidateRefreshToken(string refreshToken)
        {
            try
            {
                var user = await FindUserByRefreshToken(refreshToken);

                if (user == null)
                    return (false, "Invalid refresh token", null);

                if (user.RefreshTokenExpiry <= DateTime.Now)
                    return (false, "Refresh token expired", null);

                string newAccessToken = CreateToken(user, user.Role);
                string newRefreshToken = await GenerateRefreshToken(user);

                return (true, null, new TokenResponseDTO
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                return (false, $"Error validating refresh token: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RevokeRefreshToken(string refreshToken)
        {
            try
            {
                var user = await FindUserByRefreshToken(refreshToken);

                if (user == null)
                    return (false, "Invalid refresh token");

                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;
                await _userRepository.UpdateUserAsync(user);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error revoking refresh token: {ex.Message}");
            }
        }

        private async Task<User?> FindUserByRefreshToken(string refreshToken)
        {
            var users = await _userRepository.GetAllUsersAsync();
            return users.FirstOrDefault(u => u.RefreshToken == refreshToken);
        }
    }
}