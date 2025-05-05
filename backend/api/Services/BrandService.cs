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
        private readonly IProductRepository _productRepo;
        private readonly IProductLineRepository _productLineRepo;
        private readonly IWebHostEnvironment _env;

        public BrandService(IBrandRepository repo, IProductRepository productRepo, IProductLineRepository productLineRepo, IWebHostEnvironment webEnvironment)
        {
            _env = webEnvironment;
            _repo = repo;
            _productRepo = productRepo;
            _productLineRepo = productLineRepo;
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
                    Description = brandDTO.Description,
                    IsActive = brandDTO.IsActive ?? true,
                };

                if (brandDTO.Logo != null)
                {

                    var (success, errorMessage, path) = await ImageHelper.SaveImageAsync(brandDTO.Logo, _env.WebRootPath, "brands", 2 * 1024 * 1024);
                    if (!success)
                        return (false, errorMessage, null);

                    brand.Logo = path!;
                }

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
                    var deletedImg = ImageHelper.DeleteImage(_env.WebRootPath + brand.Logo);
                    if (!deletedImg)
                    {
                        return (false, "Error deleting old avatar image");
                    }
                    brand.Logo = string.Empty;
                }

                var productLines = await _productLineRepo.GetProductLinesByBrandIdAsync(id);
                if (productLines != null && productLines.Any())
                {
                    foreach (var productLine in productLines)
                    {
                        var products = await _productRepo.GetProductsByProductLineIdAsync(productLine.Id);
                        if (products != null && products.Any())
                        {
                            foreach (var product in products)
                            {
                                foreach (var color in product.Colors)
                                {
                                    if (color.Images != null && color.Images.Any())
                                    {
                                        foreach (var image in color.Images)
                                        {
                                            if (!string.IsNullOrEmpty(image.ImagePath))
                                            {
                                                var deleted = ImageHelper.DeleteImage(_env.WebRootPath + image.ImagePath);
                                                if (!deleted)
                                                {
                                                    return (false, "Error deleting product color image");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(productLine.Image))
                        {
                            var deleted = ImageHelper.DeleteImage(_env.WebRootPath + productLine.Image);
                            if (!deleted)
                            {
                                return (false, "Error deleting product line image");
                            }
                        }
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
                    return (false, "Not found any brands", null);

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

                if (!string.IsNullOrEmpty(brandDTO.Name))
                    brand.Name = brandDTO.Name;

                brand.Description = brandDTO.Description;

                if (brandDTO.Logo != null)
                {
                    if (!string.IsNullOrEmpty(brand.Logo))
                    {
                        var deletedImg = ImageHelper.DeleteImage(_env.WebRootPath + brand.Logo);
                        if (!deletedImg)
                            return (false, "Error deleting old logo", null);
                    }

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

        public async Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> ActivateBrandAsync(int id)
        {
            try
            {
                var brand = await _repo.GetBrandByIdAsync(id);
                if (brand == null)
                    return (false, "Brand not found", null);

                if (brand.IsActive)
                    return (true, null, brand.ToDTO());

                brand.IsActive = true;
                var success = await _repo.UpdateBrandAsync(brand);
                if (!success)
                    return (false, "Failed to activate brand", null);

                var productLines = await _productLineRepo.GetProductLinesByBrandIdAsync(id);
                if (productLines != null && productLines.Any())
                {
                    foreach (var productLine in productLines)
                    {
                        if (!productLine.IsActive && !productLine.ManuallyDeactivated)
                        {
                            productLine.IsActive = true;
                            productLine.UpdatedAt = DateTime.Now;
                            await _productLineRepo.UpdateProductLineAsync(productLine);

                            var products = await _productRepo.GetProductsByProductLineIdAsync(productLine.Id);
                            if (products != null && products.Any())
                            {
                                foreach (var product in products)
                                {
                                    if (!product.IsActive && !product.ManuallyDeactivated)
                                    {
                                        product.IsActive = true;
                                        product.UpdatedAt = DateTime.Now;
                                        await _productRepo.UpdateAsync(product);
                                    }
                                }
                            }
                        }
                    }
                }

                return (true, null, brand.ToDTO());
            }
            catch (Exception)
            {
                return (false, "Error activating brand", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, BrandDTO? Brand)> DeactivateBrandAsync(int id)
        {
            try
            {
                var brand = await _repo.GetBrandByIdAsync(id);
                if (brand == null)
                    return (false, "Brand not found", null);

                if (!brand.IsActive)
                    return (true, null, brand.ToDTO());

                brand.IsActive = false;
                var success = await _repo.UpdateBrandAsync(brand);
                if (!success)
                    return (false, "Failed to deactivate brand", null);

                var productLines = await _productLineRepo.GetProductLinesByBrandIdAsync(id);
                if (productLines != null && productLines.Any())
                {
                    foreach (var productLine in productLines)
                    {
                        if (productLine.IsActive)
                        {
                            productLine.IsActive = false;
                            productLine.UpdatedAt = DateTime.Now;
                            await _productLineRepo.UpdateProductLineAsync(productLine);

                            var products = await _productRepo.GetProductsByProductLineIdAsync(productLine.Id);
                            if (products != null && products.Any())
                            {
                                foreach (var product in products)
                                {
                                    if (product.IsActive)
                                    {
                                        product.IsActive = false;
                                        product.UpdatedAt = DateTime.Now;
                                        await _productRepo.UpdateAsync(product);
                                    }
                                }
                            }
                        }
                    }
                }

                return (true, null, brand.ToDTO());
            }
            catch (Exception)
            {
                return (false, "Error deactivating brand", null);
            }
        }
    }
}