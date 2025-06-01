using api.DTOs.Product;
using api.Queries;

namespace api.Interfaces.Services
{    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetProductsAsync(bool? isActive = null);
        Task<ProductDTO> GetProductByIdAsync(int id);
        Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO);
        Task DeleteProductAsync(int id);
        Task<ProductPagiDTO> GetPagedProductsAsync(ProductQuery productQuery);
        Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO productDTO);
        Task<ProductColorDTO> CreateProductColorAsync(int productId, CreateColorDTO productColorDTO);
        Task<ProductColorDTO> UpdateProductColorAsync(int productId, int colorId, UpdateColorDTO productColorDTO);
        Task<ProductDTO> ActivateProductAsync(int id);
        Task<ProductDTO> DeactivateProductAsync(int id);
        Task DeleteProductColorAsync(int productId, int colorId);
    }
}