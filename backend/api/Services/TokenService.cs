using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.DTOs.Auth;
using api.Helpers;
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
        private readonly IUserTokenRepository _userTokenRepository;

        public TokenService(IUserRepository userRepository, IUserTokenRepository userTokenRepository, IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
            _userRepository = userRepository;
            _userTokenRepository = userTokenRepository;
        }

        public string CreateToken(User user, string role)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("role", role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var creds = new SigningCredentials(_key, _config["JWT:Algorithm"]!);

            var tokenDescriptor = new JwtSecurityToken(
                audience: _config["JWT:Audience"],
                issuer: _config["JWT:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(_config["JWT:Expire"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            string token = TokenHelper.GenerateToken();
            string tokenHash = TokenHelper.HashToken(token);

            DateTime expiryDate = DateTime.Now.AddDays(double.Parse(_config["JWT:RefreshTokenExpiry"]!));

            var userToken = new UserToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                TokenType = "RefreshToken",
                ExpiryDate = expiryDate,
                CreatedAt = DateTime.Now
            };

            await _userTokenRepository.CreateTokenAsync(userToken);

            return token;
        }

        public async Task<string> GeneratePasswordResetToken(User user)
        {
            string token = TokenHelper.GenerateToken();
            Console.WriteLine($"[INF] Password reset token: {token}");
            string tokenHash = TokenHelper.HashToken(token);

            DateTime expiryDate = DateTime.Now.AddMinutes(15);

            var userToken = new UserToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                TokenType = "PasswordResetToken",
                ExpiryDate = expiryDate,
                CreatedAt = DateTime.Now
            };

            await _userTokenRepository.CreateTokenAsync(userToken);

            return token;
        }

        public async Task<string> GenerateEmailVerificationToken(User user)
        {
            string token = TokenHelper.GenerateToken();
            Console.WriteLine($"[INF] Email verification token: {token}");
            string tokenHash = TokenHelper.HashToken(token);

            DateTime expiryDate = DateTime.Now.AddHours(24);

            var userToken = new UserToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                TokenType = "EmailVerificationToken",
                ExpiryDate = expiryDate,
                CreatedAt = DateTime.Now
            };

            await _userTokenRepository.CreateTokenAsync(userToken);

            return token;
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? Token)> ValidateRefreshToken(string refreshToken)
        {
            try
            {
                string tokenHash = TokenHelper.HashToken(refreshToken);
                var storedToken = await _userTokenRepository.FindTokenByHashAsync(tokenHash, "RefreshToken");

                if (storedToken == null || storedToken.User == null)
                    return (false, "Invalid refresh token", null);

                if (storedToken.IsRevoked)
                    return (false, "Token has been revoked", null);

                if (storedToken.IsUsed)
                    return (false, "Token has been used", null);

                if (storedToken.ExpiryDate <= DateTime.Now)
                    return (false, "Refresh token expired", null);

                await _userTokenRepository.MarkTokenAsUsedAsync(storedToken);

                string newAccessToken = CreateToken(storedToken.User, storedToken.User.Role);
                string newRefreshToken = await GenerateRefreshToken(storedToken.User);

                return (true, null, new TokenResponseDTO
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (Exception)
            {
                return (false, $"Error validating refresh token", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RevokeRefreshToken(string refreshToken)
        {
            try
            {
                string tokenHash = TokenHelper.HashToken(refreshToken);
                var result = await _userTokenRepository.RevokeTokenAsync(tokenHash, "RefreshToken");

                if (!result)
                    return (false, "Invalid refresh token");

                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error revoking refresh token");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, User? User)> ValidatePasswordResetToken(string email, string token)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                    return (false, "User not found", null);

                string tokenHash = TokenHelper.HashToken(token);
                var storedToken = await _userTokenRepository.FindTokenByUserIdAndTypeAsync(
                    user.Id, "PasswordResetToken", tokenHash);

                if (storedToken == null)
                    return (false, "Invalid password reset token", null);

                if (storedToken.IsRevoked)
                    return (false, "Token has been revoked", null);

                if (storedToken.IsUsed)
                    return (false, "Token has been used", null);

                if (storedToken.ExpiryDate <= DateTime.Now)
                    return (false, "Password reset token has expired", null);

                await _userTokenRepository.MarkTokenAsUsedAsync(storedToken);

                return (true, null, user);
            }
            catch (Exception)
            {
                return (false, $"Error validating password reset token", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, User? User)> ValidateEmailVerificationToken(string email, string token)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                    return (false, "User not found", null);

                if (user.EmailConfirmed)
                    return (true, "Email already verified", user);

                string tokenHash = TokenHelper.HashToken(token);
                var storedToken = await _userTokenRepository.FindTokenByUserIdAndTypeAsync(
                    user.Id, "EmailVerificationToken", tokenHash);

                if (storedToken == null)
                    return (false, "Invalid email verification token", null);

                if (storedToken.IsRevoked)
                    return (false, "Token has been revoked", null);

                if (storedToken.IsUsed)
                    return (false, "Token has been used", null);

                if (storedToken.ExpiryDate <= DateTime.Now)
                    return (false, "Email verification token has expired", null);

                await _userTokenRepository.MarkTokenAsUsedAsync(storedToken);

                return (true, null, user);
            }
            catch (Exception)
            {
                return (false, $"Error validating email verification token", null);
            }
        }
    }
}