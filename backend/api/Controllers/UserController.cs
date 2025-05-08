using api.DTOs.User;
using api.Helpers;
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

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return ApiResponseHelper.Success("Users retrieved successfully", users);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return ApiResponseHelper.Success("User retrieved successfully", user);
        }

        [HttpGet("me")]
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var id = HttpContextHelper.CurrentUserId;
            var user = await _userService.GetUserByIdAsync(id);
            return ApiResponseHelper.Success("User retrieved successfully", user);
        }

        [HttpPut("me")]
        [Authorize(Roles = "admin,user")]
        [RequestSizeLimit(15 * 1024 * 1024)]
        public async Task<IActionResult> UpdateCurrentUser([FromForm] UpdateUserDTO userDTO)
        {
            var id = HttpContextHelper.CurrentUserId;
            var user = await _userService.UpdateUserAsync(id, userDTO);
            return ApiResponseHelper.Success("User updated successfully", user);
        }

        [HttpDelete("me")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> DeleteCurrentUser()
        {
            var id = HttpContextHelper.CurrentUserId;
            await _userService.DeleteUserAsync(id);
            CookieHelper.RemoveAuthTokens();
            return NoContent();
        }

        [HttpPut("{id:guid}/lock")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> LockUser([FromRoute] Guid id, [FromBody] LockUserDTO lockUserDTO)
        {
            await _userService.LockUserAsync(id, lockUserDTO, "admin");
            return ApiResponseHelper.Success<object>("User locked successfully", null);
        }

        [HttpPut("{id:guid}/unlock")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UnlockUser([FromRoute] Guid id)
        {
            await _userService.UnlockUserAsync(id);
            return ApiResponseHelper.Success<object>("User unlocked successfully", null);
        }

        [HttpPut("me/lock")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> LockCurrentUser([FromBody] LockUserDTO lockUserDTO)
        {
            var id = HttpContextHelper.CurrentUserId;
            await _userService.LockUserAsync(id, lockUserDTO, "user");

            CookieHelper.RemoveAuthTokens();
            return ApiResponseHelper.Success<object>("User locked successfully", null);
        }
    }
}