using System.Net;
using System.Text.Json;
using api.Exceptions;

namespace api.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var response = new object();

        // Xử lý các exception tùy chỉnh
        if (exception is ServerException serverException)
        {
            statusCode = serverException.StatusCode;

            response = new
            {
                Success = false,
                Message = "An unexpected error occurred",
                Detail = _env.IsDevelopment() ? serverException.Message : null,
            };
        }
        else if (exception is ApiException apiException)
        {
            statusCode = apiException.StatusCode;

            response = new
            {
                Success = false,
                apiException.Message,
            };
        }
        else
        {
            statusCode = exception switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                KeyNotFoundException => HttpStatusCode.NotFound,
                ArgumentException => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.InternalServerError,
            };

            response = new
            {
                Success = false,
                Message = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred",
                Detail = _env.IsDevelopment() ? exception.StackTrace : null
            };
        }


        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(json);
    }
}