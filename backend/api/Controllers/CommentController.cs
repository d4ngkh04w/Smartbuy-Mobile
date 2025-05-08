using api.DTOs.Comment;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/comment")]
    [ApiController]
    [Authorize(Roles = "admin,user")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);

            return Ok(new
            {
                Message = "Comment retrieved successfully",
                Comment = comment
            });
        }

        [HttpGet("product/{productId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCommentsByProductId(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var comments = await _commentService.GetCommentsByProductIdAsync(productId, page, pageSize);

            return Ok(new
            {
                Message = "Comments retrieved successfully",
                Comments = comments
            });
        }

        [HttpGet("product/{productId:int}/rating")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductAverageRating(int productId)
        {
            var rating = await _commentService.GetProductAverageRatingAsync(productId);

            return Ok(new
            {
                Message = "Product rating retrieved successfully",
                Rating = rating
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDTO commentDTO)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var comment = await _commentService.CreateCommentAsync(commentDTO, userId);

            return CreatedAtAction(nameof(GetCommentById),
                            new { id = comment.Id },
                            new
                            {
                                Message = "Comment created successfully",
                                Comment = comment
                            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentDTO commentDTO)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var comment = await _commentService.UpdateCommentAsync(id, commentDTO, userId);

            return Ok(new
            {
                Message = "Comment updated successfully",
                Comment = comment
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            await _commentService.DeleteCommentAsync(id, userId);

            return Ok(new { Message = "Comment deleted successfully" });
        }

        [HttpPost("{id:int}/reaction")]
        public async Task<IActionResult> AddReaction(int id, [FromBody] CommentReactionDTO reactionDTO)
        {
            var userId = HttpContextHelper.CurrentUserId;
            if (userId == Guid.Empty)
                throw new UnauthorizedException();

            var comment = await _commentService.AddReactionAsync(id, reactionDTO, userId);

            return Ok(new
            {
                Message = "Reaction added successfully",
                Comment = comment
            });
        }
    }
}