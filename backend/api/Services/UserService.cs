using api.DTOs.User;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;

namespace api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return (false, "User not found");
                }

                await _userRepository.DeleteUserAsync(user);

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "An error occurred while deleting the user");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<UserDTO>? Users)> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                if (users == null || !users.Any())
                {
                    return (false, "Not found", null);
                }

                var userDTOs = users.Select(u => u.ToDTO()).ToList();
                return (true, null, userDTOs);
            }
            catch (Exception)
            {
                return (false, "An error occurred while retrieving users", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, UserDTO? User)> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return (false, "User not found", null);
                }

                return (true, null, user.ToDTO());
            }
            catch (Exception)
            {
                return (false, "An error occurred while retrieving the user by ID", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(Guid id, UpdateUserDTO userDTO)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return (false, "User not found");
                }

                if (!string.IsNullOrEmpty(userDTO.Email) && userDTO.Email != user.Email)
                {
                    var emailExists = await _userRepository.UserExistsByEmailAsync(userDTO.Email);
                    if (emailExists)
                    {
                        return (false, "Email already exists");
                    }
                }
                if (!string.IsNullOrEmpty(userDTO.PhoneNumber) && userDTO.PhoneNumber != user.PhoneNumber)
                {
                    var phoneExists = await _userRepository.UserExistsByPhoneNumberAsync(userDTO.PhoneNumber);
                    if (phoneExists)
                    {
                        return (false, "Phone number already exists");
                    }
                }

                user.Name = userDTO.Name ?? user.Name;
                user.Email = userDTO.Email ?? user.Email;
                user.PhoneNumber = userDTO.PhoneNumber ?? user.PhoneNumber;
                user.Address = userDTO.Address ?? user.Address;
                user.Gender = userDTO.Gender ?? user.Gender;
                if (userDTO.Avatar != null)
                {
                    var path = Directory.GetCurrentDirectory() + user.Avatar;
                    if (File.Exists(path))
                    {
                        var deletedImg = ImageHelper.DeleteImage(path);
                        if (!deletedImg)
                        {
                            return (false, "Error deleting old image");
                        }
                    }
                    var saveImg = await ImageHelper.SaveImageAsync(userDTO.Avatar, "users", 15 * 1024 * 1024);
                    if (!saveImg.Success)
                    {
                        return (false, saveImg.ErrorMessage);
                    }
                    user.Avatar = saveImg.FilePath!;
                }

                await _userRepository.UpdateUserAsync(user);

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "An error occurred while updating the user");
            }
        }
    }
}