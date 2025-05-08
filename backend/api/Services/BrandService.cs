using api.DTOs.Brand;
using api.Exceptions;
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
        private readonly IWebHostEnvironment _env;

        public BrandService(IBrandRepository repo, IProductRepository productRepo, IProductLineRepository productLineRepo, IWebHostEnvironment webEnvironment)
        {
            _env = webEnvironment;
            _repo = repo;
            _productRepo = productRepo;
            _productLineRepo = productLineRepo;
        }

        public async Task<BrandDTO> CreateBrandAsync(CreateBrandDTO brandDTO)
        {
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

            var createdBrand = await _repo.CreateBrandAsync(brand);
            if (createdBrand == null)
                throw new ServerException("Error creating brand");

            return createdBrand.ToDTO();
        }

        public async Task DeleteBrandAsync(int id)
        {
            var brand = await _repo.GetBrandByIdAsync(id) ?? throw new NotFoundException("Brand not found");
            if (!string.IsNullOrEmpty(brand.Logo))
            {
                var deletedImg = ImageUtils.DeleteImage(_env.WebRootPath + brand.Logo);
                if (!deletedImg)
                {
                    throw new ServerException("Error deleting old avatar image");
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
                                            var deleted = ImageUtils.DeleteImage(_env.WebRootPath + image.ImagePath);
                                            if (!deleted)
                                            {
                                                throw new ServerException("Error deleting product color image");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(productLine.Image))
                    {
                        var deleted = ImageUtils.DeleteImage(_env.WebRootPath + productLine.Image);
                        if (!deleted)
                        {
                            throw new ServerException("Error deleting product line image");
                        }
                    }
                }
            }

            await _repo.DeleteBrandAsync(brand);
        }

        public async Task<BrandDTO> GetBrandByIdAsync(int id, BrandQuery query)
        {
            var brand = await _repo.GetBrandByIdAsync(id, query) ?? throw new NotFoundException("Brand not found");
            return brand.ToDTO();
        }

        public async Task<IEnumerable<BrandDTO>> GetBrandsAsync(BrandQuery query)
        {
            var brands = await _repo.GetBrandsAsync(query);

            if (brands == null || !brands.Any())
                throw new NotFoundException("Not found any brands");

            return brands.Select(b => b.ToDTO());
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
                if (!string.IsNullOrEmpty(brand.Logo))
                {
                    var deletedImg = ImageUtils.DeleteImage(_env.WebRootPath + brand.Logo);
                    if (!deletedImg)
                        throw new ServerException("Error deleting old logo image");
                }

                var path = await ImageUtils.SaveImageAsync(brandDTO.Logo, _env.WebRootPath, "brands", 2 * 1024 * 1024);
                brand.Logo = path!;
            }

            var updatedBrand = await _repo.UpdateBrandAsync(brand);
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

            return updatedBrand.ToDTO();
        }
    }
}