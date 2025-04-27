using api.DTOs.User;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/user")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userIdClaim == null)
                return Guid.Empty;

            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "No users found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new { Message = "Users retrieved successfully", result.Users });
        }

        [HttpGet("me")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var id = GetCurrentUserId();
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "User not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new { Message = "User retrieved successfully", result.User });
        }

        [HttpPut("me")]
        [Authorize(Roles = "admin,user")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> UpdateCurrentUser([FromForm] UpdateUserDTO userDTO)
        {
            var id = GetCurrentUserId();
            var result = await _userService.UpdateUserAsync(id, userDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new { Message = "User updated successfully", User = result.User });
        }

        [HttpDelete("me")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var id = GetCurrentUserId();
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            Response.Cookies.Delete("token");
            Response.Cookies.Delete("refreshToken");

            return NoContent();
        }

        [HttpPut("{id:guid}/lock")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> LockUser([FromRoute] Guid id, [FromBody] LockUserDTO lockUserDTO)
        {
            var result = await _userService.LockUserAsync(id, lockUserDTO, "admin");
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new { Message = "User locked successfully" });
        }

        [HttpPut("{id:guid}/unlock")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UnlockUser([FromRoute] Guid id)
        {
            var result = await _userService.UnlockUserAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new { Message = "User unlocked successfully" });
        }

        [HttpPut("me/lock")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> LockCurrentUser([FromBody] LockUserDTO lockUserDTO)
        {
            var id = GetCurrentUserId();
            var result = await _userService.LockUserAsync(id, lockUserDTO, "user");
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            Response.Cookies.Delete("token");
            Response.Cookies.Delete("refreshToken");
            return Ok(new { Message = "User locked successfully" });
        }
    }
}