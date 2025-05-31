using api.DTOs.User;

namespace api.Interfaces.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();
        Task<UserDTO> GetUserByIdAsync(Guid id);
        Task<UserDTO> UpdateUserAsync(Guid id, UpdateUserDTO userDTO);
        Task DeleteUserAsync(Guid id, string password);
        Task LockUserAsync(Guid id, LockUserDTO lockUserDTO, string lockedBy);
        Task UnlockUserAsync(Guid id);
    }
}