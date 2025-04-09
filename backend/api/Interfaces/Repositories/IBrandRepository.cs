using api.Models;

namespace api.Interfaces.Services
{
    public interface IBrandRepository
    {
        Task<Brand?> GetBrandByIdAsync(int id);
        Task<IEnumerable<Brand>> GetAllBrandsAsync();
        Task<Brand> CreateBrandAsync(Brand brand);
        Task<bool> UpdateBrandAsync(Brand brand);
        Task DeleteBrandAsync(Brand brand);
        Task<bool> BrandExistsAsync(string name);

    }
}