using Microsoft.AspNetCore.Mvc;

namespace api.Helpers
{
    public static class ApiResponseHelper
    {
        public static IActionResult Success<T>(string message = "Success", T? data = default)
        {
            return new OkObjectResult(new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            });
        }

        public static IActionResult Created<T>(string message = "Created successfully", T? data = default)
        {
            return new ObjectResult(new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            })
            {
                StatusCode = StatusCodes.Status201Created
            };
        }

    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}