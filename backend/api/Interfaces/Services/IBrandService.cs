using api.DTOs.Brand;

namespace api.Interfaces.Services
{
    public interface IBrandService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<BrandDTO>? Brands)> GetAllBrandsAsync();
        Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> GetBrandByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> CreateBrandAsync(CreateBrandDTO brandDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteBrandAsync(int id);
        Task<(bool Success, string? ErrorMessage)> UpdateBrandAsync(int id, UpdateBrandDTO brandDTO);
    }
}