using api.Models;

namespace api.Interfaces.Repositories
{
        public interface IProductRepository
        {
                Task<IEnumerable<Product>> GetAllAsync();
                Task<Product?> GetByIdAsync(int id);
                Task<Product> CreateAsync(Product product);
                Task<bool> UpdateAsync(Product product);
                public Task<bool> DeleteAsync(Product product);
                Task<bool> ExistsByNameAsync(string name);
                Task<(List<Product> Items, int TotalItems)> GetPagedProductsAsync(int page, int pageSize);
                Task<List<Product>> GetProductsByProductLineIdAsync(int productLineId);

                Task<ProductColor> AddColorAsync(ProductColor color);
                Task<ProductImage> AddImageAsync(ProductImage image);
        }
}