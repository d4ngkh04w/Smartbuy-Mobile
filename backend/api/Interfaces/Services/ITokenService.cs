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
        Task<(bool Success, string? ErrorMessage, TokenResponseDTO? Token)> ValidateRefreshToken(string refreshToken);
        Task<(bool Success, string? ErrorMessage)> RevokeRefreshToken(string refreshToken);
        Task<(bool Success, string? ErrorMessage, User? User)> ValidatePasswordResetToken(string email, string token);
        Task<(bool Success, string? ErrorMessage, User? User)> ValidateEmailVerificationToken(string email, string token);
    }
}