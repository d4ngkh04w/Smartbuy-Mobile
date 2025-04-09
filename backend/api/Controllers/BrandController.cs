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
        public async Task<IActionResult> GetAllBrands()
        {
            var (success, errorMessage, brands) = await _brandService.GetAllBrandsAsync();

            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = "No brands found" });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return Ok(new { Status = 200, Message = "Brand retrieved successfully", Brands = brands });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetBrandById([FromRoute] int id)
        {
            var (success, errorMessage, brand) = await _brandService.GetBrandByIdAsync(id);

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

        [HttpDelete("{id:int}")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var (success, errorMessage) = await _brandService.DeleteBrandAsync(id);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { Status = 404, Message = errorMessage });
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return NoContent();
        }

        [HttpPost]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateBrand([FromForm] CreateBrandDTO brandDTO)
        {
            var (success, errorMessage, brand) = await _brandService.CreateBrandAsync(brandDTO);
            if (!success && errorMessage != null)
            {
                if (errorMessage.Contains("Error", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(500, new { Status = 500, Message = errorMessage });
                if (errorMessage.Contains("Already exists", StringComparison.OrdinalIgnoreCase))
                    return Conflict(new { Status = 409, Message = errorMessage });
                return BadRequest(new { Status = 400, Message = errorMessage });
            }
            return CreatedAtAction(nameof(GetBrandById), new { id = brand!.Id }, new { Status = 201, Message = "Brand created successfully", Brand = brand });
        }

        [HttpPut("{id:int}")]
        // [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBrand([FromRoute] int id, [FromForm] UpdateBrandDTO brandDTO)
        {
            var (success, errorMessage) = await _brandService.UpdateBrandAsync(id, brandDTO);
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