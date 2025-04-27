using api.DTOs.Product;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/product")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _productService.GetProductsAsync();

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Products not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Products retrieved successfully",
                result.Products
            });
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById([FromRoute] int id)
        {
            var result = await _productService.GetProductByIdAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Product retrieved successfully",
                Product = result.Product
            });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDTO productDTO)
        {
            var result = await _productService.CreateProductAsync(productDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Already exists", StringComparison.OrdinalIgnoreCase) => Conflict(new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return CreatedAtAction(nameof(GetProductById),
                                new { id = result.Product!.Id },
                                new
                                {
                                    Message = "Product created successfully",
                                    Product = result.Product
                                });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromForm] UpdateProductDTO productDTO)
        {
            var result = await _productService.UpdateProductAsync(id, productDTO);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Product updated successfully",
                result.Product
            });
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Product not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return NoContent();
        }
        [HttpGet("page")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPagedProducts([FromQuery] int page, int pageSize){
            
            var result = await _productService.GetPagedProductsAsync(page, pageSize);
            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Products not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }
            return Ok(new
            {
                Message = "Products retrieved successfully",
                Products = result.ProductPagi
            });
        }
    }
}