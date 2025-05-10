namespace api.Helpers
{
    public static class CookieHelper
    {
        private static IHttpContextAccessor? _httpContextAccessor;
        public static HttpContext Current => _httpContextAccessor?.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not configured");

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static string AccessToken
        {
            get => GetCookie("token");
            set
            {
                SetCookie("token", value, ConfigHelper.JwtExpireTime);
            }
        }

        public static string RefreshToken
        {
            get => GetCookie("refreshToken");
            set
            {
                Current.Response.Cookies.Append("refreshToken", value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddDays(ConfigHelper.JwtRefreshTokenExpiry),
                    Path = "/"
                });
            }
        }

        public static void RemoveAuthTokens()
        {
            RemoveAccessToken();
            RemoveRefreshToken();
        }

        public static void RemoveAccessToken()
        {
            Current.Response.Cookies.Delete("token", new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax
            });
        }

        public static void RemoveRefreshToken()
        {
            Current.Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax
            });
        }

        public static void SetCookie(string key, string value, double expirationMinutes = 30)
        {
            Current.Response.Cookies.Append(key, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddMinutes(expirationMinutes),
                Path = "/"
            });
        }

        public static string GetCookie(string key)
        {
            return Current.Request.Cookies[key] ?? string.Empty;
        }

        public static void RemoveCookie(string key)
        {
            Current.Response.Cookies.Delete(key);
        }
    }
}