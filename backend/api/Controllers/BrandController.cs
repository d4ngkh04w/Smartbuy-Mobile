using api.DTOs.Brand;
using api.Interfaces.Services;
using api.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers.Brand
{
    [Route("api/v1/brand")]
    [ApiController]
    [Authorize]
    public class BrandController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetBrands([FromQuery] BrandQuery query)
        {
            var result = await _brandService.GetBrandsAsync(query);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Brands not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Brands retrieved successfully",
                result.Brands
            });
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBrandById([FromRoute] int id, [FromQuery] BrandQuery query)
        {
            var result = await _brandService.GetBrandByIdAsync(id, query);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Brand not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Brand retrieved successfully",
                result.Brand
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var result = await _brandService.DeleteBrandAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Brand not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateBrand([FromForm] CreateBrandDTO brandDTO)
        {
            var result = await _brandService.CreateBrandAsync(brandDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Already exists", StringComparison.OrdinalIgnoreCase) => Conflict(new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return CreatedAtAction(nameof(GetBrandById),
                                new { id = result.Brand!.Id },
                                new
                                {
                                    Message = "Brand created successfully",
                                    result.Brand
                                });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateBrand([FromRoute] int id, [FromForm] UpdateBrandDTO brandDTO)
        {
            var result = await _brandService.UpdateBrandAsync(id, brandDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Brand not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Brand updated successfully",
                result.Brand
            });
        }
    }
}