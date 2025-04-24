using api.DTOs.Auth;

namespace api.Interfaces.Services
{
    public interface IAuthService
    {
        public Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Register(RegisterDTO registerDto, string role);
        public Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Login(LoginDTO loginDto, string role);
        public Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> LoginWithGoogleAsync(GoogleLoginDTO dto, string role);
        public Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto);
        public Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto);
        public Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDTO changePasswordDto, Guid userId);
    }
}