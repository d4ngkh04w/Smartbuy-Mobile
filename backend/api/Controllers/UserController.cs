using api.DTOs.User;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/user")]
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
                if (result.ErrorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Message = "No users found" });
                if (result.ErrorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Message = result.ErrorMessage });
                return BadRequest(new { Message = result.ErrorMessage });
            }
            return Ok(result.Users);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            if (GetCurrentUserId() != id && !User.IsInRole("admin"))
                return Forbid();
            var result = await _userService.GetUserByIdAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                if (result.ErrorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Message = result.ErrorMessage });
                if (result.ErrorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Message = result.ErrorMessage });
                return BadRequest(new { Message = result.ErrorMessage });
            }
            return Ok(result.User);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "admin,user")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromForm] UpdateUserDTO userDTO)
        {
            if (GetCurrentUserId() != id && !User.IsInRole("admin"))
                return Forbid();
            var result = await _userService.UpdateUserAsync(id, userDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                if (result.ErrorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Message = result.ErrorMessage });
                if (result.ErrorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Message = result.ErrorMessage });
                return BadRequest(new { Message = result.ErrorMessage });
            }
            return Ok(new { Message = "User updated successfully" });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            if (GetCurrentUserId() != id && !User.IsInRole("admin"))
                return Forbid();
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                if (result.ErrorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Message = result.ErrorMessage });
                if (result.ErrorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Message = result.ErrorMessage });
                return BadRequest(new { Message = result.ErrorMessage });
            }
            return NoContent();
        }
    }
}