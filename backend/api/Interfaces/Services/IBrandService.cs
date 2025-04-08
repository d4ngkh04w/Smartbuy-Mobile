using api.DTOs.Brand;

namespace api.Interfaces.Services
{
    public interface IBrandService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<BrandDTO>? Brands)> GetAllBrands();
        Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> GetBrand(int? id = null, string? name = null);
        Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> CreateBrand(CreateBrandDTO brandDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteBrand(int? id = null, string? name = null);
        Task<(bool Success, string? ErrorMessage)> UpdateBrand(int id, UpdateBrandDTO brandDTO);
    }
}