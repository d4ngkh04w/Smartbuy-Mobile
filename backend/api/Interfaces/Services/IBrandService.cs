using api.DTOs.Brand;
using api.Queries;

namespace api.Interfaces.Services
{
    public interface IBrandService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<BrandDTO>? Brands)> GetBrandsAsync(BrandQuery query);
        Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> GetBrandByIdAsync(int id, BrandQuery query);
        Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> CreateBrandAsync(CreateBrandDTO brandDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteBrandAsync(int id);
        Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> UpdateBrandAsync(int id, UpdateBrandDTO brandDTO);
    }
}