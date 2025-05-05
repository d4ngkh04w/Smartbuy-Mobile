namespace api.Interfaces.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetToken);
        Task<bool> SendEmailVerificationAsync(string email, string verificationToken);
    }
}