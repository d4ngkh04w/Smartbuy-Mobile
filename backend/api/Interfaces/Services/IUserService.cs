using api.DTOs.Auth;
using api.DTOs.User;

namespace api.Interfaces.Services
{
    public interface IUserService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<UserDTO>? Users)> GetAllUsersAsync();
        Task<(bool Success, string? ErrorMessage, UserDTO? User)> GetUserByIdAsync(Guid id);
        Task<(bool Success, string? ErrorMessage, UserDTO? User)> UpdateUserAsync(Guid id, UpdateUserDTO userDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid id);
        Task<(bool Success, string? ErrorMessage)> LockUserAsync(Guid id, LockUserDTO lockUserDTO, string lockedBy);
        Task<(bool Success, string? ErrorMessage)> UnlockUserAsync(Guid id);
    }
}