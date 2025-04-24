using api.DTOs.Tag;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/tags")]
    [ApiController]
    [Authorize]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTags()
        {
            var result = await _tagService.GetAllTagsAsync();

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Tags not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Tags retrieved successfully",
                Tags = result.Tags
            });
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTagById([FromRoute] int id)
        {
            var result = await _tagService.GetTagByIdAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Tag not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Tag retrieved successfully",
                result.Tag
            });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateTag([FromBody] CreateTagDTO tagDTO)
        {
            var result = await _tagService.CreateTagAsync(tagDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Already exists", StringComparison.OrdinalIgnoreCase) => Conflict(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return CreatedAtAction(nameof(GetTagById),
                            new { id = result.Tag!.Id },
                            new
                            {
                                Message = "Tag created successfully",
                                result.Tag
                            });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateTag([FromRoute] int id, [FromBody] UpdateTagDTO tagDTO)
        {
            var result = await _tagService.UpdateTagAsync(id, tagDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Tag not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Tag updated successfully",
                result.Tag
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteTag([FromRoute] int id)
        {
            var result = await _tagService.DeleteTagAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Tag not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return NoContent();
        }
    }
}