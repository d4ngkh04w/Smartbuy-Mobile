using api.Models;
using api.Queries;

namespace api.Interfaces.Repositories
{
    public interface IProductLineRepository
    {
        Task<ProductLine?> GetProductLineByIdAsync(int id, ProductLineQuery? query = null);
        Task<IEnumerable<ProductLine>> GetProductLinesAsync(ProductLineQuery query);
        Task<ProductLine> CreateProductLineAsync(ProductLine productLine);
        Task<ProductLine> UpdateProductLineAsync(ProductLine productLine);
        Task<bool> ProductLineExistAsync(string name);
        Task<List<ProductLine>> GetProductLinesByBrandIdAsync(int brandId);
    }
}