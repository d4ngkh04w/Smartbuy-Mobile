using api.DTOs.Brand;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;
using api.Queries;

namespace api.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _repo;
        private readonly IWebHostEnvironment _env;

        public BrandService(IBrandRepository repo, IWebHostEnvironment webEnvironment)
        {
            _env = webEnvironment;
            _repo = repo;
        }

        public async Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> CreateBrandAsync(CreateBrandDTO brandDTO)
        {
            try
            {
                var newName = brandDTO.Name.Trim();
                var existsBrand = await _repo.BrandExistsAsync(newName);
                if (existsBrand)
                    return (false, "Brand already exists", null);

                var brand = new Brand
                {
                    Name = newName,
                };

                var (success, errorMessage, path) = await ImageHelper.SaveImageAsync(brandDTO.Logo, _env.WebRootPath, "brands", 2 * 1024 * 1024);
                if (!success)
                    return (false, errorMessage, null);

                brand.Logo = path!;

                var createdBrand = await _repo.CreateBrandAsync(brand);
                if (createdBrand == null)
                    return (false, "Error creating brand", null);

                return (true, null, createdBrand.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error creating brand", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteBrandAsync(int id)
        {
            try
            {
                var brand = await _repo.GetBrandByIdAsync(id);
                if (brand == null)
                    return (false, "Brand not found");

                if (!string.IsNullOrEmpty(brand.Logo))
                {
                    var deletedImg = ImageHelper.DeleteImage(Path.Combine(_env.WebRootPath, brand.Logo));
                    if (!deletedImg)
                    {
                        return (false, "Error deleting old avatar image");
                    }
                }

                await _repo.DeleteBrandAsync(brand);

                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error deleting brand");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> GetBrandByIdAsync(int id, BrandQuery query)
        {
            try
            {
                var brand = await _repo.GetBrandByIdAsync(id, query);
                if (brand == null)
                    return (false, "Brand not found", null);

                return (true, null, brand.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error retrieving brand", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<BrandDTO>? Brands)> GetBrandsAsync(BrandQuery query)
        {
            try
            {
                var brands = await _repo.GetBrandsAsync(query);

                if (brands == null || !brands.Any())
                    return (false, "Not found brands", null);

                var brandDTOs = brands.Select(b => b.ToDTO()).ToList();

                return (true, null, brandDTOs);
            }
            catch (Exception)
            {
                return (false, $"Error retrieving brands", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> UpdateBrandAsync(int id, UpdateBrandDTO brandDTO)
        {
            try
            {
                if (id <= 0)
                    return (false, "Invalid brand ID", null);

                var brand = await _repo.GetBrandByIdAsync(id);
                if (brand == null)
                    return (false, "Brand not found", null);

                var newName = brandDTO.Name?.Trim();
                if (!string.IsNullOrEmpty(newName))
                    brand.Name = newName;

                if (brandDTO.Logo != null)
                {
                    var deleted = ImageHelper.DeleteImage(Path.Combine(_env.WebRootPath, brand.Logo));
                    if (!deleted)
                        return (false, "Error deleting old logo", null);

                    var (success, errorMessage, path) = await ImageHelper.SaveImageAsync(brandDTO.Logo, _env.WebRootPath, "brands", 2 * 1024 * 1024);
                    if (!success)
                        return (false, errorMessage, null);

                    brand.Logo = path!;
                }

                var successUpdate = await _repo.UpdateBrandAsync(brand);
                if (!successUpdate)
                    return (false, "Error updating brand", null);

                return (true, null, brand.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error updating brand", null);
            }
        }
    }
}