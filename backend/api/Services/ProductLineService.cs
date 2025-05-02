using api.DTOs.ProductLine;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Queries;

namespace api.Services
{
    public class ProductLineService : IProductLineService
    {
        private readonly IProductLineRepository _productLineRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _env;

        public ProductLineService(IProductLineRepository productLineRepository, IProductRepository productRepository, IBrandRepository brandRepository, IWebHostEnvironment webEnvironment)
        {
            _productLineRepository = productLineRepository;
            _brandRepository = brandRepository;
            _productRepository = productRepository;
            _env = webEnvironment;
        }

        public async Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> CreateProductLineAsync(CreateProductLineDTO productLineDTO)
        {
            try
            {
                var brand = await _brandRepository.GetBrandByIdAsync(productLineDTO.BrandId);
                if (brand == null)
                {
                    return (false, "Brand not found", null);
                }
                bool exists = await _productLineRepository.ProductLineExistAsync(productLineDTO.Name.Trim());
                if (exists)
                {
                    return (false, "Product line already exists", null);
                }

                var result = await ImageHelper.SaveImageAsync(productLineDTO.Image, _env.WebRootPath, "product-lines", 5 * 1024 * 1024);
                if (!result.Success)
                {
                    return (false, result.ErrorMessage, null);
                }

                var productLine = productLineDTO.ToModel();
                productLine.Image = result.FilePath!;
                var createdProductLine = await _productLineRepository.CreateProductLineAsync(productLine);

                return (true, null, createdProductLine.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error creating product line", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteProductLineAsync(int id)
        {
            try
            {
                var productLine = await _productLineRepository.GetProductLineByIdAsync(id);
                if (productLine == null)
                {
                    return (false, "Product line not found");
                }

                if (!string.IsNullOrEmpty(productLine.Image))
                {
                    var deleted = ImageHelper.DeleteImage(_env.WebRootPath + productLine.Image);
                    if (!deleted)
                    {
                        return (false, "Error deleting image");
                    }
                    productLine.Image = string.Empty;
                }

                // Xoá hình ảnh của các sản phẩm trong dòng sản phẩm này
                var products = await _productRepository.GetProductsByProductLineIdAsync(id);
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

                await _productLineRepository.DeleteProductLineAsync(productLine);
                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error deleting product line");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> GetProductLineByIdAsync(int id, ProductLineQuery? query = null)
        {
            try
            {
                var productLine = await _productLineRepository.GetProductLineByIdAsync(id, query);
                if (productLine == null)
                {
                    return (false, "Product line not found", null);
                }

                return (true, null, productLine.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error retrieving product line", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<ProductLineDTO>? ProductLines)> GetProductLinesAsync(ProductLineQuery query)
        {
            try
            {
                var productLines = await _productLineRepository.GetProductLinesAsync(query);
                if (productLines == null || !productLines.Any())
                {
                    return (false, "Not found any product lines", null);
                }

                return (true, null, productLines.Select(pl => pl.ToDTO()));
            }
            catch (Exception)
            {
                return (false, $"Error retrieving product lines", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> UpdateProductLineAsync(int id, UpdateProductLineDTO productLineDTO)
        {
            try
            {
                var productLine = await _productLineRepository.GetProductLineByIdAsync(id);
                if (productLine == null)
                {
                    return (false, "Product line not found", null);
                }

                // Chỉ cập nhật các trường hợp có trong DTO
                var newName = productLineDTO.Name?.Trim();
                if (!string.IsNullOrEmpty(newName))
                {
                    productLine.Name = newName;
                }

                if (!string.IsNullOrEmpty(productLineDTO.Description?.Trim()))
                {
                    productLine.Description = productLineDTO.Description.Trim();
                }

                if (productLineDTO.IsActive.HasValue)
                {
                    productLine.IsActive = productLineDTO.IsActive.Value;
                }

                if (productLineDTO.BrandId.HasValue)
                {
                    productLine.BrandId = productLineDTO.BrandId.Value;
                }

                if (productLineDTO.Image != null)
                {
                    // Xoá hình cũ
                    var deleted = ImageHelper.DeleteImage(_env.WebRootPath + productLine.Image);
                    if (!deleted)
                    {
                        return (false, "Error deleting old image", null);
                    }
                    var res = await ImageHelper.SaveImageAsync(productLineDTO.Image, _env.WebRootPath, "product-lines", 5 * 1024 * 1024);
                    if (!res.Success)
                    {
                        return (false, res.ErrorMessage, null);
                    }

                    productLine.Image = res.FilePath!;
                }

                productLine.UpdatedAt = DateTime.Now;

                bool result = await _productLineRepository.UpdateProductLineAsync(productLine);
                if (!result)
                {
                    return (false, "Failed to update product line", null);
                }

                return (true, null, productLine.ToDTO());
            }
            catch (Exception)
            {
                return (false, $"Error updating product line", null);
            }
        }
    }
}