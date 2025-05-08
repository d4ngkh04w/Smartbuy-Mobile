using api.Utils;

namespace api.DTOs.User
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool PhoneNumberConfirmed { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public string Avatar { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public bool IsLocked { get; set; } = false;
        public string LockedBy { get; set; } = string.Empty;
        public string LockReason { get; set; } = string.Empty;
        public DateTime? LockedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtString => CreatedAt.ToRelativeTimeString();
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedAtString => UpdatedAt?.ToRelativeTimeString() ?? string.Empty;
        public DateTime? LastLogin { get; set; }
        public string LastLoginString => LastLogin?.ToRelativeTimeString() ?? string.Empty;
    }
}