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

        public static string AdminAccessToken
        {
            get => GetCookie("admin_token");
            set
            {
                Current.Response.Cookies.Append("admin_token", value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddMinutes(ConfigHelper.JwtExpireTime),
                    Path = "/",
                });
            }
        }

        public static string UserAccessToken
        {
            get => GetCookie("user_token");
            set
            {
                Current.Response.Cookies.Append("user_token", value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddMinutes(ConfigHelper.JwtExpireTime),
                    Path = "/",
                });
            }
        }

        public static string AdminRefreshToken
        {
            get => GetCookie("admin_refresh_token");
            set
            {
                Current.Response.Cookies.Append("admin_refresh_token", value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddDays(ConfigHelper.JwtRefreshTokenExpiry),
                    Path = "/",
                });
            }
        }

        public static string UserRefreshToken
        {
            get => GetCookie("user_refresh_token");
            set
            {
                Current.Response.Cookies.Append("user_refresh_token", value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.Now.AddDays(ConfigHelper.JwtRefreshTokenExpiry),
                    Path = "/",
                });
            }
        }

        public static void RemoveAuthUserTokens()
        {
            RemoveCookie("user_token");
            RemoveCookie("user_refresh_token");
        }

        public static void RemoveAuthAdminTokens()
        {
            RemoveCookie("admin_token");
            RemoveCookie("admin_refresh_token");
        }

        public static string GetCookie(string key)
        {
            return Current.Request.Cookies[key] ?? string.Empty;
        }

        public static void RemoveCookie(string key)
        {
            if (Current.Request.Cookies.ContainsKey(key))
            {
                Current.Response.Cookies.Delete(key, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                });
            }
        }
    }
}