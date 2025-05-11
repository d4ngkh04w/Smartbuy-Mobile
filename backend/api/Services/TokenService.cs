using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.DTOs.Auth;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Models;
using api.Utils;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IUserRepository _userRepository;
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IWebHostEnvironment _env;

        public TokenService(IUserRepository userRepository, IUserTokenRepository userTokenRepository, IWebHostEnvironment env)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigHelper.JwtSecretKey!));
            _userRepository = userRepository;
            _userTokenRepository = userTokenRepository;
            _env = env;
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

            var creds = new SigningCredentials(_key, ConfigHelper.JwtAlgorithm);

            var tokenDescriptor = new JwtSecurityToken(
                audience: ConfigHelper.JwtAudience,
                issuer: ConfigHelper.JwtIssuer,
                claims: claims,
                expires: DateTime.Now.AddMinutes(ConfigHelper.JwtExpireTime),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            string token = TokenUtils.GenerateToken();
            string tokenHash = TokenUtils.HashToken(token);

            var expiryDate = DateTime.Now.AddDays(ConfigHelper.JwtRefreshTokenExpiry);

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
            string token = TokenUtils.GenerateToken();
            Console.WriteLine($"[INF] Password reset token: {token}");
            string tokenHash = TokenUtils.HashToken(token);

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
            string token = TokenUtils.GenerateToken();
            Console.WriteLine($"[INF] Email verification token: {token}");
            string tokenHash = TokenUtils.HashToken(token);

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

        public async Task<TokenResponseDTO> ValidateRefreshToken(string refreshToken, string role)
        {
            string tokenHash = TokenUtils.HashToken(refreshToken);
            var storedToken = await _userTokenRepository.FindTokenByHashAsync(tokenHash, "RefreshToken");

            if (storedToken == null || storedToken.User == null)
                throw new BadRequestException("Invalid refresh token");

            if (role != storedToken.User.Role)
                return new TokenResponseDTO
                {
                    AccessToken = "",
                    RefreshToken = ""
                };

            if (storedToken.IsRevoked)
                throw new BadRequestException("Token has been revoked");

            if (storedToken.IsUsed)
                throw new BadRequestException("Token has been used");

            if (storedToken.ExpiryDate <= DateTime.Now)
                throw new BadRequestException("Refresh token expired");

            await _userTokenRepository.MarkTokenAsUsedAsync(storedToken);

            string newAccessToken = CreateToken(storedToken.User, storedToken.User.Role);
            string newRefreshToken = await GenerateRefreshToken(storedToken.User);

            return new TokenResponseDTO
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            string tokenHash = TokenUtils.HashToken(refreshToken);
            var result = await _userTokenRepository.RevokeTokenAsync(tokenHash, "RefreshToken");

            if (!result)
                throw new BadRequestException("Invalid refresh token");
        }

        public async Task<User?> ValidatePasswordResetToken(string email, string token)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                if (_env.IsDevelopment())
                {
                    throw new NotFoundException($"User not found for email: {email}");
                }
                Console.WriteLine($"[INF] User not found for email: {email}");
                return null;
            }
            string tokenHash = TokenUtils.HashToken(token);
            var storedToken = await _userTokenRepository.FindTokenByUserIdAndTypeAsync(
                user.Id, "PasswordResetToken", tokenHash);
            if (storedToken == null)
            {
                if (_env.IsDevelopment())
                {
                    throw new BadRequestException("Invalid password reset token");
                }
                Console.WriteLine($"[INF] Invalid password reset token for email: {email}");
                return null;
            }

            if (storedToken.IsRevoked)
                throw new BadRequestException("Token has been revoked");

            if (storedToken.IsUsed)
                throw new BadRequestException("Token has been used");

            if (storedToken.ExpiryDate <= DateTime.Now)
                throw new BadRequestException("Password reset token has expired");

            await _userTokenRepository.MarkTokenAsUsedAsync(storedToken);

            return user;
        }

        public async Task<User?> ValidateEmailVerificationToken(string email, string token)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                Console.WriteLine($"[INF] User not found for email: {email}");
                return null;
            }

            if (user.EmailConfirmed)
                return user;

            string tokenHash = TokenUtils.HashToken(token);
            var storedToken = await _userTokenRepository.FindTokenByUserIdAndTypeAsync(
                user.Id, "EmailVerificationToken", tokenHash);

            if (storedToken == null)
            {
                Console.WriteLine($"[INF] Invalid email verification token for email: {email}");
                return null;
            }

            if (storedToken.IsRevoked)
                throw new BadRequestException("Token has been revoked");

            if (storedToken.IsUsed)
                throw new BadRequestException("Token has been used");

            if (storedToken.ExpiryDate <= DateTime.Now)
                throw new BadRequestException("Email verification token has expired");

            await _userTokenRepository.MarkTokenAsUsedAsync(storedToken);

            return user;
        }
    }
}