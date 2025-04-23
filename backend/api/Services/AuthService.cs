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

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Register(Register registerDto, string role)
        {
            // Validate unique phone number and email
            if (await _userRepository.UserExistsByPhoneNumberAsync(registerDto.PhoneNumber))
                return (false, "Phone number already exists", null);

            if (await _userRepository.UserExistsByEmailAsync(registerDto.Email))
                return (false, "Email already exists", null);

            try
            {
                // Create new user with hashed password
                var user = new User
                {
                    PhoneNumber = registerDto.PhoneNumber,
                    Email = registerDto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = role
                };

                await _userRepository.CreateUserAsync(user);

                // Generate authentication tokens
                var token = _tokenService.CreateToken(user, role);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);

                return (true, null, new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return (false, $"Error during registration: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> Login(Login loginDto, string role)
        {
            try
            {
                // Find user by phone number
                var user = await _userRepository.GetUserByPhoneNumberAsync(loginDto.PhoneNumber);

                // Validate credentials
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                {
                    return (false, "Invalid phone number or password", null);
                }

                // Validate user role
                if (user.Role != role)
                {
                    return (false, $"Your account does not have access as a '{role}'", null);
                }

                // Update last login time
                user.LastLogin = DateTime.Now;
                await _userRepository.UpdateUserAsync(user);

                // Generate authentication tokens
                var token = _tokenService.CreateToken(user, role);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);

                return (true, "Login successful", new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return (false, $"Error during login: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, TokenResponseDTO? token)> LoginWithGoogleAsync(GoogleLogin dto, string role)
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
                    // Tao người dùng mới nếu chưa tồn tại
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

                // Tạo token xác thực
                var token = _tokenService.CreateToken(user, role);
                var refreshToken = await _tokenService.GenerateRefreshToken(user);

                user.LastLogin = DateTime.Now;
                await _userRepository.UpdateUserAsync(user);

                return (true, "Login with Google successful", new TokenResponseDTO { Token = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                return (false, $"Error during Google login: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ForgotPasswordAsync(ForgotPasswordDTO forgotPasswordDto)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    return (true, null);
                }

                // Xác thực token reset mật khẩu và thiết lập thời gian hết hạn
                var resetToken = _tokenService.GeneratePasswordResetToken();
                Console.WriteLine($"[INF] Password reset token: {resetToken}"); // For debugging purposes
                user.PasswordResetToken = resetToken;
                user.PasswordResetTokenExpiry = DateTime.Now.AddMinutes(5);
                await _userRepository.UpdateUserAsync(user);

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
                return (false, $"Error processing password reset request: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDTO resetPasswordDto)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    // Không trả về thông báo lỗi nếu người dùng không tồn tại
                    // để tránh lộ thông tin người dùng
                    return (false, "Invalid reset request");
                }

                // Xác thực token reset mật khẩu và thiết lập thời gian hết hạn
                if (user.PasswordResetToken != resetPasswordDto.Token)
                {
                    return (false, "Invalid reset token");
                }

                // Kiểm tra thời gian hết hạn của token
                if (user.PasswordResetTokenExpiry == null ||
                    !_tokenService.ValidatePasswordResetToken(resetPasswordDto.Token, user.PasswordResetTokenExpiry.Value))
                {
                    return (false, "Password reset token has expired");
                }

                // Cập nhật mật khẩu của người dùng và xóa token reset
                user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.Password);
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;
                await _userRepository.UpdateUserAsync(user);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error resetting password: {ex.Message}");
            }
        }
    }
}