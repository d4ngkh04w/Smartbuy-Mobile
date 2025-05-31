using api.DTOs.ProductLine;
using api.Queries;

namespace api.Interfaces.Services
{
    public interface IProductLineService
    {
        Task<ProductLineDTO> GetProductLineByIdAsync(int id, ProductLineQuery? query = null);
        Task<IEnumerable<ProductLineDTO>> GetProductLinesAsync(ProductLineQuery query);
        Task<ProductLineDTO> CreateProductLineAsync(CreateProductLineDTO productLineDTO);
        Task<ProductLineDTO> UpdateProductLineAsync(int id, UpdateProductLineDTO productLineDTO);
        Task<ProductLineDTO> ActivateProductLineAsync(int id);
        Task<ProductLineDTO> DeactivateProductLineAsync(int id);
    }
}