using api.DTOs.Product;
using api.Exceptions;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;
using api.Queries;
using api.Utils;

namespace api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductLineRepository _productLineRepository;
        private readonly IWebHostEnvironment _env;

        public ProductService(IProductRepository productRepository,
                              IProductLineRepository productLineRepository,
                              IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _productLineRepository = productLineRepository;
            _env = webHostEnvironment;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();

            if (!products.Any())
                return new List<ProductDTO>();

            return products.Select(p => p.ToProductDTO());
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            return product.ToProductDTO();
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO)
        {
            if (await _productRepository.ExistsByNameAsync(productDTO.Name.Trim()))
                throw new AlreadyExistsException("Product with this name already exists");
            else
                productDTO.Name = productDTO.Name.Trim();

            // Validate that product line exists
            var productLine = await _productLineRepository.GetProductLineByIdAsync(productDTO.ProductLineId) ?? throw new NotFoundException($"Product line with ID {productDTO.ProductLineId} not found");
            if (!productLine.IsActive)
                throw new BadRequestException($"Product line with ID {productDTO.ProductLineId} is inactive");

            var product = productDTO.ToProductModel();
            product.Detail = productDTO.ToProductDetailModel();

            var createdProduct = await _productRepository.CreateAsync(product);
            return createdProduct.ToProductDTO();
        }

        public async Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO productDTO)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");

            // Update basic info
            if (!string.IsNullOrEmpty(productDTO.Name?.Trim()))
                product.Name = productDTO.Name.Trim();

            if (productDTO.ImportPrice.HasValue)
                product.ImportPrice = productDTO.ImportPrice.Value;

            if (productDTO.SalePrice.HasValue)
                product.SalePrice = productDTO.SalePrice.Value;

            if (!string.IsNullOrEmpty(productDTO.Description?.Trim()))
                product.Description = productDTO.Description.Trim();

            if (productDTO.ProductLineId.HasValue)
                product.ProductLineId = productDTO.ProductLineId.Value;

            // Update details if they exist
            if (product.Detail != null)
            {
                if (!string.IsNullOrEmpty(productDTO.Warranty?.Trim()))
                    product.Detail.WarrantyMonths = int.Parse(productDTO.Warranty);

                if (!string.IsNullOrEmpty(productDTO.RAM?.Trim()))
                    product.Detail.RAMInGB = int.Parse(productDTO.RAM);

                if (!string.IsNullOrEmpty(productDTO.Storage?.Trim()))
                    product.Detail.StorageInGB = int.Parse(productDTO.Storage);

                if (!string.IsNullOrEmpty(productDTO.ScreenSize?.Trim()))
                    product.Detail.ScreenSizeInch = decimal.Parse(productDTO.ScreenSize);

                if (!string.IsNullOrEmpty(productDTO.ScreenResolution?.Trim()))
                    product.Detail.ScreenResolution = productDTO.ScreenResolution.Trim();

                if (!string.IsNullOrEmpty(productDTO.Battery?.Trim()))
                    product.Detail.BatteryCapacityMAh = int.Parse(productDTO.Battery);

                if (!string.IsNullOrEmpty(productDTO.OS?.Trim()))
                    product.Detail.OperatingSystem = productDTO.OS.Trim();

                if (!string.IsNullOrEmpty(productDTO.Processor?.Trim()))
                    product.Detail.Processor = productDTO.Processor.Trim();

                if (productDTO.SimSlots.HasValue)
                    product.Detail.SimSlots = productDTO.SimSlots.Value;
            }

            // Save basic product changes first
            await _productRepository.UpdateAsync(product);

            // Handle color updates
            if (productDTO.UpdateColorData != null && productDTO.UpdateColorData.Any())
            {
                foreach (var colorData in productDTO.UpdateColorData)
                {
                    ProductColor? color = null;

                    // If color has an ID, find and update the existing color
                    if (colorData.Id.HasValue && colorData.Id.Value > 0)
                    {
                        color = product.Colors.FirstOrDefault(c => c.Id == colorData.Id.Value);

                        if (color != null && !string.IsNullOrEmpty(colorData.Name?.Trim()))
                        {
                            color.Name = colorData.Name.Trim();
                            await _productRepository.UpdateAsync(product); // Update color name
                        }
                    }
                    // Otherwise create a new color
                    else if (!string.IsNullOrEmpty(colorData.Name?.Trim()))
                    {
                        color = new ProductColor
                        {
                            Name = colorData.Name.Trim(),
                            ProductId = product.Id
                        };

                        // Save the new color to get valid ID
                        color = await _productRepository.AddColorAsync(color);
                    }

                    if (color != null)
                    {
                        // Process added images for this color
                        if (colorData.AddImages != null && colorData.AddImages.Any())
                        {
                            for (int i = 0; i < colorData.AddImages.Count; i++)
                            {
                                var image = colorData.AddImages[i];
                                var filePath = await ImageUtils.SaveImageAsync(image, _env.WebRootPath, "products", 5 * 1024 * 1024);

                                var newImage = new ProductImage
                                {
                                    ImagePath = filePath,
                                    IsMain = i == colorData.MainImageIndex && colorData.SetMainImage,
                                    ColorId = color.Id
                                };

                                // Add image directly to database
                                await _productRepository.AddImageAsync(newImage);
                            }
                        }

                        // Process main image setting
                        if (colorData.MainImageId.HasValue && colorData.SetMainImage)
                        {
                            // Get all images for this color from DB to ensure they're up to date
                            var dbProduct = await _productRepository.GetByIdAsync(id);
                            if (dbProduct != null)
                            {
                                var dbColor = dbProduct.Colors.FirstOrDefault(c => c.Id == color.Id);

                                if (dbColor != null && dbColor.Images.Any())
                                {
                                    foreach (var image in dbColor.Images)
                                    {
                                        // Update IsMain status
                                        image.IsMain = image.Id == colorData.MainImageId.Value;
                                    }

                                    // Save changes
                                    await _productRepository.UpdateAsync(dbProduct);
                                }
                            }
                        }

                        // Process image deletion
                        if (colorData.RemoveImageIds != null && colorData.RemoveImageIds.Any())
                        {
                            // Get fresh data from DB
                            var dbProduct = await _productRepository.GetByIdAsync(id);

                            if (dbProduct != null)
                            {
                                var dbColor = dbProduct.Colors.FirstOrDefault(c => c.Id == color.Id);

                                if (dbColor != null)
                                {
                                    foreach (var imageId in colorData.RemoveImageIds)
                                    {
                                        var image = dbColor.Images.FirstOrDefault(i => i.Id == imageId);
                                        if (image != null)
                                        {
                                            var deletedImg = ImageUtils.DeleteImage(_env.WebRootPath + image.ImagePath);
                                            if (!deletedImg)
                                            {
                                                Console.WriteLine($"Warning: Failed to delete image file: {image.ImagePath}");
                                            }
                                            dbColor.Images.Remove(image);
                                        }
                                    }

                                    // Save changes
                                    await _productRepository.UpdateAsync(dbProduct);
                                }
                            }
                        }
                    }
                }
            }

            // Handle color deletion
            if (productDTO.RemoveColorIds != null && productDTO.RemoveColorIds.Any())
            {
                // Get fresh product data
                var dbProduct = await _productRepository.GetByIdAsync(id);

                if (dbProduct != null)
                {
                    foreach (var colorId in productDTO.RemoveColorIds)
                    {
                        var color = dbProduct.Colors.FirstOrDefault(c => c.Id == colorId);
                        if (color != null)
                        {
                            // Delete all images associated with this color
                            foreach (var image in color.Images.ToList())
                            {
                                if (!string.IsNullOrEmpty(image.ImagePath))
                                {
                                    var deletedImg = ImageUtils.DeleteImage(_env.WebRootPath + image.ImagePath);
                                    if (!deletedImg)
                                    {
                                        Console.WriteLine($"Warning: Failed to delete image file: {image.ImagePath}");
                                    }
                                }
                                color.Images.Remove(image);
                            }

                            // Remove the color
                            dbProduct.Colors.Remove(color);
                        }
                    }

                    // Save changes
                    await _productRepository.UpdateAsync(dbProduct);
                }
            }

            // Get the final updated product
            var updatedProduct = await _productRepository.GetByIdAsync(id);
            return updatedProduct!.ToProductDTO();
        }

        public async Task DeleteProductAsync(int id)
        {
            // Get product first to check if it exists and to access its images
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            await _productRepository.DeleteAsync(product);
        }
        public async Task<ProductPagiDTO> GetPagedProductsAsync(ProductQuery productQuery)
        {
            var (items, totalItems) = await _productRepository.GetPagedProductsAsync(productQuery);

            if (items == null || !items.Any())
            {
                return new ProductPagiDTO
                {
                    TotalItems = 0,
                    Items = new List<ProductSummaryDTO>()
                };
            }
            var productSummaries = items.Select(ProductMapper.ToSummaryDTO).ToList();
            var result = new ProductPagiDTO
            {
                TotalItems = totalItems,
                Items = productSummaries
            };

            return result;
        }

        public async Task<ProductColorDTO> CreateProductColorAsync(int productId, CreateColorDTO productColorDTO)
        {
            var product = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");
            var productColor = new ProductColor
            {
                Name = productColorDTO.Name.Trim(),
                Quantity = productColorDTO.Quantity,
                ProductId = product.Id
            };

            var savedColor = await _productRepository.AddColorAsync(productColor);

            if (productColorDTO.Images != null && productColorDTO.Images.Any())
            {
                for (int i = 0; i < productColorDTO.Images.Count; i++)
                {
                    var image = productColorDTO.Images[i];
                    var filePath = await ImageUtils.SaveImageAsync(image, _env.WebRootPath, "products", 5 * 1024 * 1024); // 5 MB max size

                    var productImage = new ProductImage
                    {
                        ImagePath = filePath,
                        IsMain = i == productColorDTO.MainImageIndex,
                        ColorId = savedColor.Id
                    };

                    await _productRepository.AddImageAsync(productImage);
                }
            }

            return savedColor.ToProductColorDTO();
        }

        public async Task<ProductDTO> ActivateProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            var productLine = await _productLineRepository.GetProductLineByIdAsync(product.ProductLineId) ?? throw new NotFoundException("Product line not found");

            if (!productLine.IsActive)
                throw new BadRequestException("Cannot activate product because parent product line is inactive");

            if (product.IsActive)
                throw new BadRequestException("Product is already active");

            product.IsActive = true;
            product.ManuallyDeactivated = false;
            product.UpdatedAt = DateTime.Now;

            var result = await _productRepository.UpdateAsync(product);

            return result.ToProductDTO();
        }

        public async Task<ProductDTO> DeactivateProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            if (!product.IsActive)
                return product.ToProductDTO();

            product.IsActive = false;
            product.ManuallyDeactivated = true;
            product.UpdatedAt = DateTime.Now;

            var result = await _productRepository.UpdateAsync(product);

            return result.ToProductDTO();
        }
    }
}