using api.DTOs.Product;

namespace api.Interfaces.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetProductsAsync();
        Task<ProductDTO> GetProductByIdAsync(int id);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO);
        Task DeleteProductAsync(int id);
        Task<ProductPagiDTO> GetPagedProductsAsync(int page, int pageSize, string? search = null, string? sortBy = "newest", string? brand = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO productDTO);
        Task<ProductColorDTO> CreateProductColorAsync(int productId, CreateColorDTO productColorDTO);
        Task<ProductDTO> ActivateProductAsync(int id);
        Task<ProductDTO> DeactivateProductAsync(int id);
    }
}