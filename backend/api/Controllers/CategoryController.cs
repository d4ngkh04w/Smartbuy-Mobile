using api.DTOs.Category;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var (success, errorMessage, categories) = await _categoryService.GetAllCategoriesAsync();

            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = "No categories found" });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Category retrieved successfully", Categories = categories });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById([FromRoute] int id)
        {
            var (success, errorMessage, category) = await _categoryService.GetCategoryByIdAsync(id);

            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Category retrieved successfully", Category = category });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDTO categoryDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Status = 400, Message = "Invalid data" });
            var (success, errorMessage, category) = await _categoryService.CreateCategoryAsync(categoryDTO);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { Status = 409, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return CreatedAtAction(nameof(GetCategoryById), new { id = category?.Id }, new { Status = 201, Message = "Category created successfully", Category = category });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] UpdateCategoryDTO categoryDTO)
        {
            var (success, errorMessage) = await _categoryService.UpdateCategoryAsync(id, categoryDTO);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not Found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Category updated successfully" });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int id)
        {
            var (success, errorMessage) = await _categoryService.DeleteCategoryAsync(id);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not Found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return NoContent();
        }
    }
}