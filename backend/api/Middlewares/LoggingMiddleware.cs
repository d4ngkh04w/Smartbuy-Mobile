using api.Helpers;

namespace api.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Request: {requestMethod} {requestPath} | User: {userEmail} | IP: {userIP} | User-Agent: {userAgent}",
                context.Request.Method,
                context.Request.Path,
                HttpContextHelper.CurrentUserEmail,
                HttpContextHelper.UserIP,
                HttpContextHelper.UserAgent);

            await _next(context);
        }
    }
}