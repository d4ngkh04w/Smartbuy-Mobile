using api.Models;

namespace api.Interfaces.Services
{
    public interface IBrandRepository
    {
        Task<Brand?> GetBrandAsync(int? id = null, string? name = null);
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<Brand> CreateBrandAsync(Brand brand);
        Task<bool> UpdateBrandAsync(Brand brand);
        Task<bool> DeleteBrandAsync(int? id = null, string? name = null);
    }
}