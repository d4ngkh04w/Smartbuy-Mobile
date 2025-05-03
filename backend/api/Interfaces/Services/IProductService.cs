using api.DTOs.Product;

namespace api.Interfaces.Services
{
    public interface IProductService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<ProductDTO>? Products)> GetProductsAsync();
        Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> GetProductByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> CreateProductAsync(CreateProductDTO productDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(int id);
        Task<(bool Success, string? ErrorMessage, ProductPagiDTO? ProductPagi)> GetPagedProductsAsync(int page, int pageSize);
        Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> UpdateProductAsync(int id, UpdateProductDTO productDTO);
        Task<(bool Success, string? ErrorMessage, ProductColorDTO? ProductColor)> CreateProductColorAsync(int productId, CreateColorDTO productColorDTO);
    }
}