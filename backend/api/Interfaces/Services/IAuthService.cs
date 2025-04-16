using api.DTOs.Auth;

namespace api.Interfaces.Services
{
    public interface IAuthService
    {
        public Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Register(Register registerDto, string role);
        public Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Login(Login loginDto, string role);
        public Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> LoginWithGoogleAsync(GoogleLogin dto, string role);
    }
}