using api.DTOs.Brand;
using api.Helpers;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository repo;
        private readonly ILogger<BrandService> logger;
        public BrandService(IBrandRepository repo, ILogger<BrandService> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }

        public async Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> CreateBrandAsync(CreateBrandDTO brandDTO)
        {
            try
            {
                var existsBrand = await repo.BrandExistsAsync(brandDTO.Name);
                if (existsBrand)
                    return (false, "Brand already exists", null);

                var brand = new Brand
                {
                    Name = brandDTO.Name,
                };

                var (success, errorMessage, path) = await ImageHelper.SaveImageAsync(brandDTO.Logo, "brands", 2 * 1024 * 1024);
                if (!success)
                    return (false, errorMessage, null);

                brand.Logo = path!;

                var createdBrand = await repo.CreateBrandAsync(brand);
                if (createdBrand == null)
                    return (false, "Error creating brand", null);

                return (true, null, createdBrand.ToDTO());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating brand");
                return (false, "Error creating brand", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteBrandAsync(int id)
        {
            try
            {
                var brand = await repo.GetBrandByIdAsync(id);
                if (brand == null)
                    return (false, "Brand not found");

                var deleted = ImageHelper.DeleteImage(Directory.GetCurrentDirectory() + brand.Logo);
                if (!deleted)
                    return (false, "Error deleting brand logo");

                await repo.DeleteBrandAsync(brand);

                return (true, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting brand");
                return (false, "Error deleting brand");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<BrandDTO>? Brands)> GetAllBrandsAsync()
        {
            try
            {
                var brands = await repo.GetAllBrandsAsync();
                if (brands == null || brands.Count() == 0)
                    return (false, "Not found", null);
                var brandDTOs = brands.Select(b => b.ToDTO()).ToList();
                return (true, null, brandDTOs);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all brands");
                return (false, "Error retrieving all brands", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> GetBrandByIdAsync(int id)
        {
            try
            {
                var brand = await repo.GetBrandByIdAsync(id);
                if (brand == null)
                    return (false, "Brand not found", null);

                return (true, null, brand.ToDTO());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving brand");
                return (false, "Error retrieving brand", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateBrandAsync(int id, UpdateBrandDTO brandDTO)
        {
            try
            {
                if (id <= 0)
                    return (false, "Invalid brand ID");

                var brand = await repo.GetBrandByIdAsync(id);
                if (brand == null)
                    return (false, "Brand not found");

                if (!string.IsNullOrEmpty(brandDTO.Name))
                    brand.Name = brandDTO.Name;

                if (brandDTO.Logo != null)
                {
                    var deleted = ImageHelper.DeleteImage(Directory.GetCurrentDirectory() + brand.Logo);
                    if (!deleted)
                        return (false, "Error deleting old logo");
                    var (success, errorMessage, path) = await ImageHelper.SaveImageAsync(brandDTO.Logo, "brands", 2 * 1024 * 1024);
                    if (!success)
                        return (false, errorMessage);

                    brand.Logo = path!;
                }

                var successUpdate = await repo.UpdateBrandAsync(brand);
                if (!successUpdate)
                    return (false, "Error updating brand");

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating brand");
                return (false, "Error updating brand");
            }
        }
    }
}