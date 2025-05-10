using api.DTOs.Product;
using api.Helpers;
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
            var products = await _productService.GetProductsAsync();
            return ApiResponseHelper.Success("Products retrieved successfully", products);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductById([FromRoute] int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return ApiResponseHelper.Success("Product retrieved successfully", product);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDTO productDTO)
        {
            var product = await _productService.CreateProductAsync(productDTO);
            return ApiResponseHelper.Created("Product created successfully", product);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromForm] UpdateProductDTO productDTO)
        {
            var product = await _productService.UpdateProductAsync(id, productDTO);
            return ApiResponseHelper.Success("Product updated successfully", product);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        [HttpGet("page")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPagedProducts([FromQuery] int page, int pageSize, string? search = null, string? sortBy = "newest", string? brand = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var product = await _productService.GetPagedProductsAsync(page, pageSize, search, sortBy, brand, minPrice, maxPrice);
            return ApiResponseHelper.Success("Products retrieved successfully", product);
        }


        [HttpPost("{productId:int}/color")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateProductColor([FromRoute] int productId, [FromForm] CreateColorDTO productColorDTO)
        {
            var color = await _productService.CreateProductColorAsync(productId, productColorDTO);
            return ApiResponseHelper.Created("Product color created successfully", color);
        }

        [HttpPut("{id:int}/activate")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActivateProduct([FromRoute] int id)
        {
            var product = await _productService.ActivateProductAsync(id);
            return ApiResponseHelper.Success("Product activated successfully", product);
        }

        [HttpPut("{id:int}/deactivate")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeactivateProduct([FromRoute] int id)
        {
            var product = await _productService.DeactivateProductAsync(id);
            return ApiResponseHelper.Success("Product deactivated successfully", product);
        }
    }
}