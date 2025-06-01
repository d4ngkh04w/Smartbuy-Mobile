using api.DTOs.Brand;
using api.Queries;

namespace api.Interfaces.Services
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandDTO>> GetBrandsAsync(BrandQuery query);
        Task<BrandDTO> GetBrandByIdAsync(int id, BrandQuery query);
        Task<BrandDTO> CreateBrandAsync(CreateBrandDTO brandDTO);
        Task<BrandDTO> UpdateBrandAsync(int id, UpdateBrandDTO brandDTO);
        Task<BrandDTO> ActivateBrandAsync(int id);
        Task<BrandDTO> DeactivateBrandAsync(int id);
    }
}