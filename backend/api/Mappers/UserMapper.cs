using api.DTOs.User;
using api.Models;
using api.Utils;

namespace api.Mappers
{
    public static class UserMapper
    {
        public static UserDTO ToDTO(this User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Address = user.Address,
                DateOfBirth = user.DateOfBirth,
                CreatedAt = DateTimeUtils.FormatDateTime(user.CreatedAt),
                UpdatedAt = user.UpdatedAt.HasValue ? DateTimeUtils.FormatDateTime(user.UpdatedAt.Value) : null,
                IsLocked = user.IsLocked,
                LockedBy = user.LockedBy ?? string.Empty,
                LockReason = user.LockReason ?? string.Empty,
                LockedAt = user.LockedAt,
                Gender = user.Gender ?? string.Empty,
                Name = user.Name,
                Avatar = user.Avatar ?? string.Empty,
                LastLogin = user.LastLogin.HasValue ? DateTimeUtils.FormatDateTime(user.LastLogin.Value) : null,
            };
        }
    }
}