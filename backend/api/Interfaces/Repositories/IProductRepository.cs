using api.Models;
using api.Queries;

namespace api.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(Product product);
        Task<bool> DeleteAsync(Product product);
        Task<bool> ExistsByNameAsync(string name);
        Task<(List<Product> Items, int TotalItems)> GetPagedProductsAsync(ProductQuery productQuery);
        Task<List<Product>> GetProductsByProductLineIdAsync(int productLineId);
        Task<ProductColor?> GetProductColorAsync(int productId, int colorId);
        Task<ProductColor> AddColorAsync(ProductColor color);
        Task<ProductColor> UpdateColorAsync(ProductColor color);
        Task<IEnumerable<ProductImage>> AddImagesAsync(IEnumerable<ProductImage> images);
        Task DeleteColorAsync(ProductColor color);
    }
}