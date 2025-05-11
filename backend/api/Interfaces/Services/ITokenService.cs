using api.DTOs.Auth;
using api.Models;

namespace api.Interfaces.Services
{
    public interface ITokenService
    {
        string CreateToken(User user, string role);
        Task<string> GenerateRefreshToken(User user);
        Task<string> GeneratePasswordResetToken(User user);
        Task<string> GenerateEmailVerificationToken(User user);
        Task<TokenResponseDTO> ValidateRefreshToken(string refreshToken, string role);
        Task RevokeRefreshToken(string refreshToken);
        Task<User?> ValidatePasswordResetToken(string email, string token);
        Task<User?> ValidateEmailVerificationToken(string email, string token);
    }
}