using api.DTOs.User;

namespace api.Interfaces.Services
{
    public interface IUserService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<UserDTO>? Users)> GetAllUsersAsync();
        Task<(bool Success, string? ErrorMessage, UserDTO? User)> GetUserByIdAsync(Guid id);
        // Task<(bool Success, string? ErrorMessage, UserDTO? User)> GetUserByPhoneNumberAsync(string phoneNumber);
        // Task<(bool Success, string? ErrorMessage, UserDTO? User)> GetUserByEmailAsync(string email);
        Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(Guid id, UpdateUserDTO userDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid id);
    }
}