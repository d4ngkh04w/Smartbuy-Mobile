using api.DTOs.Auth;
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
        private readonly IWebHostEnvironment _env;

        public UserService(IUserRepository userRepository, IWebHostEnvironment webEnvironment)
        {
            _env = webEnvironment;
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

                // Xóa ảnh đại diện của người dùng nếu tồn tại
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var deletedImg = ImageHelper.DeleteImage(Path.Combine(_env.WebRootPath, user.Avatar));
                    if (!deletedImg)
                    {
                        return (false, "Error deleting avatar image");
                    }
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

        public async Task<(bool Success, string? ErrorMessage)> LockUserAsync(Guid id, LockUserDTO lockUserDTO, string lockedBy)
        {

            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return (false, "User not found");
                }

                user.IsLocked = true;
                user.LockReason = lockUserDTO.Reason?.Trim() ?? user.LockReason;
                user.LockedBy = lockedBy;
                user.LockedAt = DateTime.Now;

                await _userRepository.UpdateUserAsync(user);
                return (true, null);
            }
            catch (Exception)
            {
                return (false, "An error occurred while locking the user");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UnlockUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return (false, "User not found");
                }

                user.IsLocked = false;
                user.LockReason = null;
                user.LockedBy = null;
                user.LockedAt = null;

                await _userRepository.UpdateUserAsync(user);
                return (true, null);
            }
            catch (Exception)
            {
                return (false, "An error occurred while unlocking the user");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, UserDTO? User)> UpdateUserAsync(Guid id, UpdateUserDTO userDTO)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return (false, "User not found", null);
                }

                // Xác thực tính duy nhất của email
                if (!string.IsNullOrEmpty(userDTO.Email) && userDTO.Email != user.Email)
                {
                    var emailExists = await _userRepository.UserExistsByEmailAsync(userDTO.Email);
                    if (emailExists)
                    {
                        return (false, "Email already exists", null);
                    }
                }

                // Xác thực tính duy nhất của số điện thoại
                if (!string.IsNullOrEmpty(userDTO.PhoneNumber) && userDTO.PhoneNumber != user.PhoneNumber)
                {
                    var phoneExists = await _userRepository.UserExistsByPhoneNumberAsync(userDTO.PhoneNumber.Trim());
                    {
                        return (false, "Phone number already exists", null);
                    }
                }

                // Cập nhật thông tin người dùng
                user.Name = userDTO.Name?.Trim() ?? user.Name;
                user.Email = userDTO.Email ?? user.Email;
                user.PhoneNumber = userDTO.PhoneNumber ?? user.PhoneNumber;
                user.Address = userDTO.Address?.Trim() ?? user.Address;
                user.Gender = userDTO.Gender?.Trim() ?? user.Gender;

                if (userDTO.Avatar != null)
                {
                    // Xóa ảnh đại diện cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(user.Avatar))
                    {
                        string path = Path.Combine(_env.WebRootPath, user.Avatar);
                        var deletedImg = ImageHelper.DeleteImage(path);
                        if (!deletedImg)
                        {
                            return (false, "Error deleting old avatar image", null);
                        }
                    }

                    // Lưu ảnh đại diện mới
                    var saveImg = await ImageHelper.SaveImageAsync(userDTO.Avatar, _env.WebRootPath, "users", 15 * 1024 * 1024);
                    if (!saveImg.Success)
                    {
                        return (false, saveImg.ErrorMessage, null);
                    }
                    user.Avatar = saveImg.FilePath!;
                }

                user.UpdatedAt = DateTime.Now;

                await _userRepository.UpdateUserAsync(user);
                return (true, null, user.ToDTO());
            }
            catch (Exception)
            {
                return (false, "An error occurred while updating the user", null);
            }
        }
    }
}