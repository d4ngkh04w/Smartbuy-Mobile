// using api.DTOs.Tag;
// using api.Helpers;
// using api.Interfaces.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace api.Controllers
// {
//     [Route("api/v1/tags")]
//     [ApiController]
//     [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
//     public class TagController : ControllerBase
//     {
//         private readonly ITagService _tagService;

//         public TagController(ITagService tagService)
//         {
//             _tagService = tagService;
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<IActionResult> GetAllTags()
//         {
//             var tags = await _tagService.GetAllTagsAsync();
//             return ApiResponseHelper.Success("Tags retrieved successfully", tags);
//         }

//         [HttpGet("{id:int}")]
//         [AllowAnonymous]
//         public async Task<IActionResult> GetTagById([FromRoute] int id)
//         {
//             var tag = await _tagService.GetTagByIdAsync(id);
//             return ApiResponseHelper.Success("Tag retrieved successfully", tag);
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateTag([FromBody] CreateTagDTO tagDTO)
//         {
//             var tag = await _tagService.CreateTagAsync(tagDTO);
//             return ApiResponseHelper.Created("Tag created successfully", tag);
//         }

//         [HttpPut("{id:int}")]
//         public async Task<IActionResult> UpdateTag([FromRoute] int id, [FromBody] UpdateTagDTO tagDTO)
//         {
//             var tag = await _tagService.UpdateTagAsync(id, tagDTO);
//             return ApiResponseHelper.Success("Tag updated successfully", tag);
//         }

//         [HttpDelete("{id:int}")]
//         public async Task<IActionResult> DeleteTag([FromRoute] int id)
//         {
//             await _tagService.DeleteTagAsync(id);
//             return NoContent();
//         }
//     }
// }