using api.DTOs.Auth;

namespace api.Interfaces
{
    public interface IAuthService
    {
        public Task<(bool success, string? errors)> Register(Register registerDto, string role);
        public Task<(bool success, string? token)> Login(Login loginDto, string role);
    }
}