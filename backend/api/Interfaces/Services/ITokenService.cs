using api.DTOs.Auth;
using api.Models;

namespace api.Interfaces.Services
{
    public interface ITokenService
    {
        // Tạo JWT token
        string CreateToken(User user, string role);

        // Tạo và lưu refresh token
        Task<string> GenerateRefreshToken(User user);

        // Tạo và lưu token đặt lại mật khẩu
        Task<string> GeneratePasswordResetToken(User user);

        // Tạo và lưu token xác thực email
        Task<string> GenerateEmailVerificationToken(User user);

        // Xác thực và làm mới token 
        Task<(bool Success, string? ErrorMessage, TokenResponseDTO? Token)> ValidateRefreshToken(string refreshToken);

        // Thu hồi refresh token 
        Task<(bool Success, string? ErrorMessage)> RevokeRefreshToken(string refreshToken);

        // Xác thực token đặt lại mật khẩu
        Task<(bool Success, string? ErrorMessage, User? User)> ValidatePasswordResetToken(string email, string token);

        // Xác thực token xác thực email
        Task<(bool Success, string? ErrorMessage, User? User)> ValidateEmailVerificationToken(string email, string token);
    }
}