using api.DTOs.Auth;
using api.Models;

namespace api.Interfaces.Services
{
    public interface ITokenService
    {
        Task<string> GenerateRefreshToken(User user);
        string CreateToken(User user, string role);
        Task<(bool Success, string? ErrorMessage, TokenResponseDTO? Token)> ValidateRefreshToken(string refreshToken);
    }
}