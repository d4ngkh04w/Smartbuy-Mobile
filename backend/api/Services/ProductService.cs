using api.DTOs.Product;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductLineRepository _productLineRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IWebHostEnvironment _env;

        public ProductService(IProductRepository productRepository,
                              IProductLineRepository productLineRepository,
                              IBrandRepository brandRepository,
                              IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _productLineRepository = productLineRepository;
            _brandRepository = brandRepository;
            _env = webHostEnvironment;
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<ProductDTO>? Products)> GetProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();

                if (!products.Any())
                    return (false, "Not found any products", null);

                var productDTOs = products.Select(p => p.ToProductDTO()).ToList();
                return (true, null, productDTOs);
            }
            catch (Exception)
            {
                return (false, $"Error retrieving products", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);

                if (product == null)
                    return (false, "Product not found", null);

                var productDTO = product.ToProductDTO();
                return (true, null, productDTO);
            }
            catch (Exception)
            {
                return (false, $"Error retrieving product", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> CreateProductAsync(CreateProductDTO productDTO)
        {
            try
            {
                if (await _productRepository.ExistsByNameAsync(productDTO.Name.Trim()))
                    return (false, "Product with this name already exists", null);
                else
                    productDTO.Name = productDTO.Name.Trim();

                // Validate that brand exists
                // var brand = await _brandRepository.GetBrandByIdAsync(productDTO.BrandId);
                // if (brand == null)
                //     return (false, $"Brand with ID {productDTO.BrandId} not found", null);

                // if (!brand.IsActive)
                //     return (false, $"Brand with ID {productDTO.BrandId} is inactive", null);

                // Validate that product line exists
                var productLine = await _productLineRepository.GetProductLineByIdAsync(productDTO.ProductLineId);
                if (productLine == null)
                    return (false, $"Product line with ID {productDTO.ProductLineId} not found", null);

                if (!productLine.IsActive)
                    return (false, $"Product line with ID {productDTO.ProductLineId} is inactive", null);

                // Validate that product line belongs to the selected brand
                // if (productLine.BrandId != productDTO.BrandId)
                //     return (false, $"Product line with ID {productDTO.ProductLineId} does not belong to brand with ID {productDTO.BrandId}", null);

                var product = productDTO.ToProductModel();
                product.Detail = productDTO.ToProductDetailModel();

                var createdProduct = await _productRepository.CreateAsync(product);
                return (true, null, createdProduct.ToProductDTO());
            }
            catch (Exception)
            {
                return (false, $"Error creating product", null);
            }
            // try
            // {
            //     if (await _productRepository.ExistsByNameAsync(productDTO.Name.Trim()))
            //         return (false, "Product with this name already exists", null);
            //     else
            //         productDTO.Name = productDTO.Name.Trim();

            //     // Validate that brand exists
            //     var brand = await _brandRepository.GetBrandByIdAsync(productDTO.BrandId);
            //     if (brand == null)
            //         return (false, $"Brand with ID {productDTO.BrandId} not found", null);

            //     if (!brand.IsActive)
            //         return (false, $"Brand with ID {productDTO.BrandId} is inactive", null);

            //     // Validate that product line exists
            //     var productLine = await _productLineRepository.GetProductLineByIdAsync(productDTO.ProductLineId);
            //     if (productLine == null)
            //         return (false, $"Product line with ID {productDTO.ProductLineId} not found", null);

            //     if (!productLine.IsActive)
            //         return (false, $"Product line with ID {productDTO.ProductLineId} is inactive", null);

            //     // Validate that product line belongs to the selected brand
            //     if (productLine.BrandId != productDTO.BrandId)
            //         return (false, $"Product line with ID {productDTO.ProductLineId} does not belong to brand with ID {productDTO.BrandId}", null);

            //     var product = productDTO.ToProductModel();

            //     // Chỉ thêm thông số kỹ thuật nếu các trường chính được cung cấp
            //     if (productDTO.RAM.HasValue && productDTO.Storage.HasValue &&
            //         productDTO.ScreenSize.HasValue && productDTO.Battery.HasValue &&
            //         !string.IsNullOrEmpty(productDTO.OS) && !string.IsNullOrEmpty(productDTO.Processor) &&
            //         !string.IsNullOrEmpty(productDTO.ScreenResolution))
            //     {
            //         product.Detail = productDTO.ToProductDetailModel();
            //     }

            //     // Create product first to get the ID
            //     var createdProduct = await _productRepository.CreateAsync(product);

            //     // Process color data with their associated images
            //     if (productDTO.ColorData != null && productDTO.ColorData.Any())
            //     {
            //         foreach (var colorData in productDTO.ColorData)
            //         {
            //             // Create color for the product
            //             var color = new ProductColor
            //             {
            //                 Name = colorData.Name,
            //                 ProductId = createdProduct.Id
            //             };

            //             // Save color to database first to get a valid ID
            //             var savedColor = await _productRepository.AddColorAsync(color);

            //             // Process images for this color
            //             if (colorData.Images != null && colorData.Images.Any())
            //             {
            //                 for (int i = 0; i < colorData.Images.Count; i++)
            //                 {
            //                     var image = colorData.Images[i];
            //                     var res = await ImageHelper.SaveImageAsync(image, _env.WebRootPath, "products", 5 * 1024 * 1024); // 5 MB max size
            //                     if (!res.Success)
            //                         return (false, res.ErrorMessage, null);

            //                     // Create image with valid ColorId
            //                     var productImage = new ProductImage
            //                     {
            //                         ImagePath = res.FilePath!,
            //                         IsMain = i == colorData.MainImageIndex,
            //                         ColorId = savedColor.Id // Use the ID from the saved color
            //                     };

            //                     // Save image to database
            //                     await _productRepository.AddImageAsync(productImage);
            //                 }
            //             }
            //         }
            //     }

            //     // Get the updated product with all related data
            //     var result = await _productRepository.GetByIdAsync(createdProduct.Id);
            //     return (true, null, result!.ToProductDTO());
            // }
            // catch (Exception ex)
            // {
            //     return (false, $"Error creating product: {ex.Message}", null);
            // }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> UpdateProductAsync(int id, UpdateProductDTO productDTO)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);

                if (product == null)
                    return (false, "Product not found", null);

                // Update basic info
                if (!string.IsNullOrEmpty(productDTO.Name?.Trim()))
                    product.Name = productDTO.Name.Trim();

                if (productDTO.Quantity.HasValue)
                    product.Quantity = productDTO.Quantity.Value;

                if (productDTO.ImportPrice.HasValue)
                    product.ImportPrice = productDTO.ImportPrice.Value;

                if (productDTO.SalePrice.HasValue)
                    product.SalePrice = productDTO.SalePrice.Value;

                if (!string.IsNullOrEmpty(productDTO.Description?.Trim()))
                    product.Description = productDTO.Description.Trim();

                if (productDTO.IsActive.HasValue)
                    product.IsActive = productDTO.IsActive.Value;

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
                                    var res = await ImageHelper.SaveImageAsync(image, _env.WebRootPath, "products", 5 * 1024 * 1024);
                                    if (!res.Success)
                                        return (false, res.ErrorMessage, null);

                                    var newImage = new ProductImage
                                    {
                                        ImagePath = res.FilePath!,
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
                                                var deletedImg = ImageHelper.DeleteImage(_env.WebRootPath + image.ImagePath);
                                                if (!deletedImg)
                                                {
                                                    // Just log warning but continue
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
                                        var deletedImg = ImageHelper.DeleteImage(_env.WebRootPath + image.ImagePath);
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
                return updatedProduct != null
                    ? (true, null, updatedProduct.ToProductDTO())
                    : (false, "Error retrieving updated product", null);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating product: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(int id)
        {
            try
            {
                // Get product first to check if it exists and to access its images
                var product = await _productRepository.GetByIdAsync(id);

                if (product == null)
                    return (false, "Product not found");

                // Delete associated images
                // if (product.Images != null && product.Images.Any())
                // {
                //     foreach (var image in product.Images.ToList())
                //     {
                //         if (!string.IsNullOrEmpty(image.ImagePath))
                //         {
                //             bool success = ImageHelper.DeleteImage(_env.WebRootPath + image.ImagePath);
                //             if (!success)
                //             {
                //                 // Log the error but continue with product deletion
                //                 Console.WriteLine($"[WARN]: Failed to delete image file: {image.ImagePath}");
                //             }
                //         }
                //     }
                // }

                // Proceed with product deletion
                var result = await _productRepository.DeleteAsync(product);
                return result ? (true, null) : (false, "Error deleting product from database");
            }
            catch (Exception)
            {
                return (false, $"Error deleting product");
            }
        }
        public async Task<(bool Success, string? ErrorMessage, ProductPagiDTO? ProductPagi)> GetPagedProductsAsync(int page, int pageSize)
        {
            try
            {
                var (items, totalItems) = await _productRepository.GetPagedProductsAsync(page, pageSize);

                if (items == null || !items.Any())
                {
                    return (false, "Not found any products", null);
                }
                var productSummaries = items.Select(ProductMapper.ToSummaryDTO).ToList();
                var result = new ProductPagiDTO
                {
                    TotalItems = totalItems,
                    Items = productSummaries
                };

                return (true, null, result);
            }
            catch (Exception)
            {
                return (false, $"Error retrieving paged products", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductColorDTO? ProductColor)> CreateProductColorAsync(int productId, CreateColorDTO productColorDTO)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId);

                if (product == null)
                    return (false, "Product not found", null);

                var productColor = new ProductColor
                {
                    Name = productColorDTO.Name.Trim(),
                    ProductId = product.Id
                };

                var savedColor = await _productRepository.AddColorAsync(productColor);

                if (productColorDTO.Images != null && productColorDTO.Images.Any())
                {
                    for (int i = 0; i < productColorDTO.Images.Count; i++)
                    {
                        var image = productColorDTO.Images[i];
                        var res = await ImageHelper.SaveImageAsync(image, _env.WebRootPath, "products", 5 * 1024 * 1024); // 5 MB max size
                        if (!res.Success)
                            return (false, res.ErrorMessage, null);

                        var productImage = new ProductImage
                        {
                            ImagePath = res.FilePath!,
                            IsMain = i == productColorDTO.MainImageIndex,
                            ColorId = savedColor.Id
                        };

                        await _productRepository.AddImageAsync(productImage);
                    }
                }

                return (true, null, savedColor.ToProductColorDTO());
            }
            catch (Exception)
            {
                return (false, $"Error creating product color", null);
            }
        }
    }
}