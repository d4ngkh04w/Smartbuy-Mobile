using api.DTOs.Auth;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Models;
using Google.Apis.Auth;

namespace api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly string _googleClientId;

        public AuthService(IUserRepository userRepository, ITokenService tokenService, IEmailService emailService, IConfiguration config)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _googleClientId = config["Google:ClientId"]!;
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Register(RegisterDTO registerDto, string role)
        {
            // Validate unique phone number and email
            if (await _userRepository.UserExistsByPhoneNumberAsync(registerDto.PhoneNumber.Trim()))
                return (false, "Phone number already exists", null);

            if (await _userRepository.UserExistsByEmailAsync(registerDto.Email))
                return (false, "Email already exists", null);

            try
            {
                var user = new User
                {
                    PhoneNumber = registerDto.PhoneNumber.Trim(),
                    Email = registerDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = role,
                    LastLogin = DateTime.Now,
                    EmailConfirmed = false // Đặt trạng thái chưa xác thực email
                };

                await _userRepository.CreateUserAsync(user);

                // Tạo access token và refresh token
                var refreshToken = await _tokenService.GenerateRefreshToken(user);
                var token = _tokenService.CreateToken(user, role);

                // Không gửi email xác thực ngay lúc đăng ký nữa
                // Chỉ trả về thông tin token để người dùng đăng nhập

                return (true, null, new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error during registration: {ex.Message}");
                return (false, $"Error during registration", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Login(LoginDTO loginDto, string role)
        {
            try
            {
                var user = await _userRepository.GetUserByPhoneNumberAsync(loginDto.PhoneNumber);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    return (false, "Invalid phone number or password", null);
                }

                // Kiểm tra trạng thái khóa tài khoản
                if (user.IsLocked)
                {
                    string lockMessage = user.LockedBy == "user"
                        ? "Your account has been temporarily locked by yourself.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it"
                        : $"Your account has been restricted by administrator.\nReason: {user.LockReason}.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it";

                    return (false, lockMessage, null);
                }

                if (user.Role != role)
                {
                    return (false, $"Your account does not have access as a '{role}'", null);
                }

                // Update last login
                user.LastLogin = DateTime.Now;
                await _userRepository.UpdateUserAsync(user);

                // Tạo access token và refresh token
                var refreshToken = await _tokenService.GenerateRefreshToken(user);
                var token = _tokenService.CreateToken(user, role);

                return (true, "Login successful", new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error during login: {ex.Message}");
                return (false, $"Error during login", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> LoginWithGoogleAsync(GoogleLoginDTO dto, string role)
        {
            try
            {
                // Xác thực token Google
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new List<string> { _googleClientId },
                });

                if (payload == null)
                {
                    return (false, "Invalid Google token", null);
                }

                // Kiểm tra xem người dùng đã tồn tại trong hệ thống chưa
                var user = await _userRepository.GetUserByEmailAsync(payload.Email);
                if (user == null)
                {
                    // Tạo người dùng mới nếu chưa tồn tại
                    user = new User
                    {
                        PhoneNumber = string.Empty,
                        Email = payload.Email,
                        Name = payload.Name ?? string.Empty,
                        Password = string.Empty,
                        Role = role,
                        EmailConfirmed = true
                    };

                    await _userRepository.CreateUserAsync(user);
                }
                else if (user.Role != role)
                {
                    return (false, $"Your account does not have access as a '{role}'", null);
                }
                else if (user.IsLocked)
                {
                    string lockMessage = user.LockedBy == "user"
                        ? "Your account has been temporarily locked by yourself.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it"
                        : $"Your account has been restricted by administrator.\nReason: {user.LockReason}.\nPlease contact support at smartbuymobile.team@gmail.com to unlock it";

                    return (false, lockMessage, null);
                }

                // Tạo token xác thực
                var token = _tokenService.CreateToken(user, role);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);

                user.LastLogin = DateTime.Now;
                await _userRepository.UpdateUserAsync(user);

                return (true, "Login with Google successful", new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error during Google login: {ex.Message}");
                return (false, $"Error during Google login", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    Console.WriteLine($"[INF] User not found for email: {forgotPasswordDto.Email}");
                    return (true, null);
                }

                // Tạo token đặt lại mật khẩu
                var resetToken = await _tokenService.GeneratePasswordResetToken(user);
                Console.WriteLine($"[INF] Password reset token: {resetToken}"); // For debugging

                // Gửi email reset mật khẩu với token
                var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
                if (!emailSent)
                {
                    return (false, "Failed to send password reset email");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error processing password reset request: {ex.Message}");
                return (false, $"Error processing password reset request");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
        {
            try
            {
                // Xác thực token và lấy thông tin người dùng
                var (success, message, user) = await _tokenService.ValidatePasswordResetToken(resetPasswordDto.Email, resetPasswordDto.Token);

                if (!success || user == null)
                {
                    // Nếu token không hợp lệ hoặc hết hạn, trả về thông báo lỗi
                    return (false, message);
                }

                // Cập nhật mật khẩu mới
                user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.Password);
                await _userRepository.UpdateUserAsync(user);

                return (true, "Password has been reset successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error resetting password: {ex.Message}");
                return (false, $"Error resetting password");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(ChangePasswordDTO changePasswordDto, Guid userId)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return (false, "User not found");
                }

                // Kiểm tra mật khẩu cũ
                if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.OldPassword, user.Password))
                {
                    return (false, "Old password is incorrect");
                }

                // Cập nhật mật khẩu mới
                user.Password = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                await _userRepository.UpdateUserAsync(user);

                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error changing password");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> SendEmailVerificationAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return (false, "User not found");
                }

                if (user.EmailConfirmed)
                {
                    return (true, "Email already verified");
                }

                // Tạo token xác thực email
                var verificationToken = await _tokenService.GenerateEmailVerificationToken(user);

                // Gửi email xác thực với token
                var emailSent = await _emailService.SendEmailVerificationAsync(user.Email, verificationToken);
                if (!emailSent)
                {
                    return (false, "Failed to send verification email");
                }

                return (true, "Verification email sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error sending email verification: {ex.Message}");
                return (false, "Error sending verification email");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> VerifyEmailAsync(VerifyEmailDTO verifyEmailDto)
        {
            try
            {
                // Xác thực token và lấy thông tin người dùng
                var (success, message, user) = await _tokenService.ValidateEmailVerificationToken(verifyEmailDto.Email, verifyEmailDto.Token);

                if (!success || user == null)
                {
                    return (false, message);
                }

                // Xác nhận email
                user.EmailConfirmed = true;
                await _userRepository.UpdateUserAsync(user);

                return (true, "Email verified successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error verifying email: {ex.Message}");
                return (false, "Error verifying email");
            }
        }
    }
}