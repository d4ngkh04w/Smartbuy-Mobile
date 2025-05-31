using api.DTOs.Brand;
using api.Exceptions;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;
using api.Queries;
using api.Utils;

namespace api.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _repo;
        private readonly IProductRepository _productRepo;
        private readonly IProductLineRepository _productLineRepo;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);
        private readonly IWebHostEnvironment _env;

        public BrandService(
            IBrandRepository repo,
            IProductRepository productRepo,
            IProductLineRepository productLineRepo,
            IWebHostEnvironment webEnvironment,
            ICacheService cacheService)
        {
            _cacheService = cacheService;
            _env = webEnvironment;
            _repo = repo;
            _productRepo = productRepo;
            _productLineRepo = productLineRepo;
        }

        public async Task<BrandDTO> CreateBrandAsync(CreateBrandDTO brandDTO)
        {
            if (brandDTO.Logo != null && !brandDTO.Logo.IsImage())
            {
                throw new BadRequestException("Invalid image format");
            }

            var newName = brandDTO.Name.Trim();
            var existsBrand = await _repo.BrandExistsAsync(newName);
            if (existsBrand)
                throw new AlreadyExistsException("Brand already exists");

            var brand = new Brand
            {
                Name = newName,
                Description = brandDTO.Description,
                IsActive = brandDTO.IsActive ?? true,
            };

            if (brandDTO.Logo != null)
            {
                var filePath = await ImageUtils.SaveImageAsync(brandDTO.Logo, _env.WebRootPath, "brands", 2 * 1024 * 1024);
                brand.Logo = filePath;
            }
            var createdBrand = await _repo.CreateBrandAsync(brand) ?? throw new ServerException("Error creating brand");

            _cacheService.RemoveAllBrandsCache();

            return createdBrand.ToDTO();
        }

        public async Task<BrandDTO> GetBrandByIdAsync(int id, BrandQuery query)
        {
            string cacheKey = CacheKeyManager.GetBrandKey(id);

            if (_cacheService.TryGetValue(cacheKey, out BrandDTO? cachedBrand) && cachedBrand != null)
            {
                return cachedBrand;
            }

            var brand = await _repo.GetBrandByIdAsync(id, query) ?? throw new NotFoundException("Brand not found");
            var brandDto = brand.ToDTO();

            _cacheService.Set(cacheKey, brandDto, _cacheDuration);

            return brandDto;
        }
        public async Task<IEnumerable<BrandDTO>> GetBrandsAsync(BrandQuery query)
        {
            string cacheKey = CacheKeyManager.GetBrandsKey(query);

            if (_cacheService.TryGetValue(cacheKey, out IEnumerable<BrandDTO>? cachedBrands) && cachedBrands != null)
            {
                return cachedBrands;
            }

            var brands = await _repo.GetBrandsAsync(query);

            if (brands == null || !brands.Any())
                throw new NotFoundException("Not found any brands");

            var brandsDto = brands.Select(b => b.ToDTO()).ToList();

            _cacheService.Set(cacheKey, brandsDto, _cacheDuration);

            return brandsDto;
        }

        public async Task<BrandDTO> UpdateBrandAsync(int id, UpdateBrandDTO brandDTO)
        {
            if (id <= 0)
                throw new BadRequestException("Invalid brand ID");

            var brand = await _repo.GetBrandByIdAsync(id) ?? throw new NotFoundException("Brand not found");
            if (!string.IsNullOrEmpty(brandDTO.Name))
                brand.Name = brandDTO.Name;

            brand.Description = brandDTO.Description;

            if (brandDTO.Logo != null)
            {
                if (!brandDTO.Logo.IsImage())
                    throw new BadRequestException("Invalid image format");

                if (!string.IsNullOrEmpty(brand.Logo))
                {
                    await ImageUtils.DeleteImageAsync(_env.WebRootPath + brand.Logo);
                }

                var path = await ImageUtils.SaveImageAsync(brandDTO.Logo, _env.WebRootPath, "brands", 2 * 1024 * 1024);
                brand.Logo = path!;
            }
            var updatedBrand = await _repo.UpdateBrandAsync(brand);

            _cacheService.RemoveBrandCache(id);
            _cacheService.RemoveAllBrandsCache();

            return updatedBrand.ToDTO();
        }

        public async Task<BrandDTO> ActivateBrandAsync(int id)
        {
            var brand = await _repo.GetBrandByIdAsync(id) ?? throw new NotFoundException("Brand not found");
            if (brand.IsActive)
                return brand.ToDTO();

            brand.IsActive = true;
            var updatedBrand = await _repo.UpdateBrandAsync(brand);

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

            _cacheService.RemoveBrandCache(id);
            _cacheService.RemoveAllBrandsCache();

            return updatedBrand.ToDTO();
        }

        public async Task<BrandDTO> DeactivateBrandAsync(int id)
        {
            var brand = await _repo.GetBrandByIdAsync(id) ?? throw new NotFoundException("Brand not found");
            if (!brand.IsActive)
                return brand.ToDTO();

            brand.IsActive = false;
            var updatedBrand = await _repo.UpdateBrandAsync(brand);

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

            _cacheService.RemoveBrandCache(id);
            _cacheService.RemoveAllBrandsCache();

            return updatedBrand.ToDTO();
        }
    }
}