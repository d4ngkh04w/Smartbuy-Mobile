using api.DTOs.ProductLine;
using api.Queries;

namespace api.Interfaces.Services
{
    public interface IProductLineService
    {
        Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> GetProductLineByIdAsync(int id, ProductLineQuery? query = null);
        Task<(bool Success, string? ErrorMessage, IEnumerable<ProductLineDTO>? ProductLines)> GetProductLinesAsync(ProductLineQuery query);
        Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> CreateProductLineAsync(CreateProductLineDTO productLineDTO);
        Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> UpdateProductLineAsync(int id, UpdateProductLineDTO productLineDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteProductLineAsync(int id);
    }
}