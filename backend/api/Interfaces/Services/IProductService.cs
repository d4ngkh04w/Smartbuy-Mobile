using api.DTOs.Product;
using api.Queries;

namespace api.Interfaces.Services
{
    public interface IProductService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<ProductDTO>? Products)> GetProductsAsync();
        Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> GetProductByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> CreateProductAsync(CreateProductDTO productDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(int id);
        Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(int id, UpdateProductDTO productDTO);
    }
}