namespace api.Helpers
{
    public static class ConfigHelper
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string GoogleClientId => _configuration?["Google:ClientId"] ?? throw new InvalidOperationException("Google Client ID not found in configuration");
        public static string JwtSecretKey => _configuration?["JWT:Key"] ?? throw new InvalidOperationException("JWT Secret Key not found in configuration");
        public static string JwtIssuer => _configuration?["JWT:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found in configuration");
        public static string JwtAudience => _configuration?["JWT:Audience"] ?? throw new InvalidOperationException("JWT Audience not found in configuration");
        public static string JwtAlgorithm => _configuration?["JWT:Algorithm"] ?? throw new InvalidOperationException("JWT Algorithm not found in configuration");
        public static double JwtExpireTime => double.TryParse(_configuration?["JWT:Expire"], out var expire) ? expire : throw new InvalidOperationException("JWT Expire not found in configuration");
        public static double JwtRefreshTokenExpiry => double.TryParse(_configuration?["JWT:RefreshTokenExpiry"], out var refreshTokenExpiry) ? refreshTokenExpiry : throw new InvalidOperationException("JWT Refresh Token Expiry not found in configuration");
        public static string EmailDisplayName => _configuration?["EmailSettings:DisplayName"] ?? throw new InvalidOperationException("Email Display Name not found in configuration");
        public static string EmailSender => _configuration?["EmailSettings:From"] ?? throw new InvalidOperationException("Email Sender not found in configuration");
        public static string EmailHost => _configuration?["EmailSettings:Host"] ?? throw new InvalidOperationException("Email Host not found in configuration");
        public static int EmailPort => int.TryParse(_configuration?["EmailSettings:Port"], out var port) ? port : throw new InvalidOperationException("Email Port not found in configuration");
        public static string Email => _configuration?["EmailSettings:Email"] ?? throw new InvalidOperationException("Email not found in configuration");
        public static string EmailPassword => _configuration?["EmailSettings:Password"] ?? throw new InvalidOperationException("Email Password not found in configuration");
    }
}