using api.DTOs.ProductLine;
using api.Interfaces.Services;
using api.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/product-line")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class ProductLineController : ControllerBase
    {
        private readonly IProductLineService _productLineService;

        public ProductLineController(IProductLineService productLineService)
        {
            _productLineService = productLineService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductLines([FromQuery] ProductLineQuery query)
        {
            var result = await _productLineService.GetProductLinesAsync(query);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Product lines retrieved successfully",
                result.ProductLines
            });
        }

        [HttpGet("by-brand/{brandId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductLinesByBrand([FromRoute] int brandId, [FromQuery] ProductLineQuery query)
        {
            query.BrandId = brandId;

            var result = await _productLineService.GetProductLinesAsync(query);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Product lines for brand retrieved successfully",
                result.ProductLines
            });
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductLineById([FromRoute] int id, [FromQuery] ProductLineQuery query)
        {
            var result = await _productLineService.GetProductLineByIdAsync(id, query);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product line not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Product line retrieved successfully",
                result.ProductLine
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductLine([FromForm] CreateProductLineDTO productLineDTO)
        {
            var result = await _productLineService.CreateProductLineAsync(productLineDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Already exists", StringComparison.OrdinalIgnoreCase) => Conflict(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return CreatedAtAction(nameof(GetProductLineById),
                            new { id = result.ProductLine!.Id },
                            new
                            {
                                Message = "Product line created successfully",
                                result.ProductLine
                            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProductLine([FromRoute] int id, [FromForm] UpdateProductLineDTO productLineDTO)
        {
            var result = await _productLineService.UpdateProductLineAsync(id, productLineDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product line not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Product line updated successfully",
                result.ProductLine
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProductLine([FromRoute] int id)
        {
            var result = await _productLineService.DeleteProductLineAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product line not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return NoContent();
        }

        [HttpPut("{id:int}/activate")]
        public async Task<IActionResult> ActivateProductLine([FromRoute] int id)
        {
            var result = await _productLineService.ActivateProductLineAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product line not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Product line activated successfully",
                result.ProductLine
            });
        }

        [HttpPut("{id:int}/deactivate")]
        public async Task<IActionResult> DeactivateProductLine([FromRoute] int id)
        {
            var result = await _productLineService.DeactivateProductLineAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product line not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Product line deactivated successfully",
                result.ProductLine
            });
        }
    }
}