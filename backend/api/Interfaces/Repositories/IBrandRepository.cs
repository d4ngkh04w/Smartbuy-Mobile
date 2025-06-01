using api.Models;
using api.Queries;

namespace api.Interfaces.Repositories
{
    public interface IBrandRepository
    {
        Task<Brand?> GetBrandByIdAsync(int id, BrandQuery? query = null);
        Task<IEnumerable<Brand>> GetBrandsAsync(BrandQuery query);
        Task<Brand> CreateBrandAsync(Brand brand);
        Task<Brand> UpdateBrandAsync(Brand brand);
        Task<bool> BrandExistsAsync(string name);
    }
}