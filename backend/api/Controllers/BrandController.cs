using api.DTOs.Brand;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Brand
{
    [Route("api/brand")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBrand([FromQuery] int? id = null, [FromQuery] string? name = null)
        {
            bool success;
            string? errorMessage;
            object? brand;

            if (id == null && name == null)
                (success, errorMessage, brand) = await _brandService.GetAllBrands();
            else
                (success, errorMessage, brand) = await _brandService.GetBrand(id, name);

            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Brand retrieved successfully", Brand = brand });
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBrand([FromQuery] int? id = null, [FromQuery] string? name = null)
        {
            var (success, errorMessage) = await _brandService.DeleteBrand(id, name);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Brand deleted successfully" });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateBrand([FromForm] CreateBrandDTO brandDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Status = 400, Message = "Invalid data" });
            var (success, errorMessage, brand) = await _brandService.CreateBrand(brandDTO);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Brand created successfully", Brand = brand });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBrand([FromQuery] int id, [FromForm] UpdateBrandDTO brandDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Status = 400, Message = "Invalid data" });
            var (success, errorMessage) = await _brandService.UpdateBrand(id, brandDTO);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Brand updated successfully" });
        }
    }
}