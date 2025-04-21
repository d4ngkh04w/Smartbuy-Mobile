using api.DTOs.Product;
using api.Helpers;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;
using Microsoft.AspNetCore.Hosting;

namespace api.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductService(IProductRepository productRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<ProductDTO>? Products)> GetProductsAsync()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                
                if (!products.Any())
                    return (false, "Products not found", null);

                var productDTOs = products.Select(p => p.ToProductDTO()).ToList();
                return (true, null, productDTOs);
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving products: {ex.Message}", null);
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
            catch (Exception ex)
            {
                return (false, $"Error retrieving product: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductDTO? Product)> CreateProductAsync(CreateProductDTO productDTO)
        {
            try
            {
                // Check if product with same name already exists
                if (await _productRepository.ExistsByNameAsync(productDTO.Name))
                    return (false, "Product with this name already exists", null);

                // Create new product model from DTO
                var product = productDTO.ToProductModel();
                
                // Add product details
                product.Detail = productDTO.ToProductDetailModel();

                // Add colors
                if (productDTO.Colors != null && productDTO.Colors.Any())
                {
                    foreach (var color in productDTO.Colors)
                    {
                        product.Colors.Add(new ProductColor { Name = color });
                    }
                }

                // Create product first to get the ID
                var createdProduct = await _productRepository.CreateAsync(product);

                // Add images
                if (productDTO.Images != null && productDTO.Images.Any())
                {
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products", createdProduct.Id.ToString());
                    
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (var image in productDTO.Images)
                    {
                        var fileName = await ImageHelper.SaveImageAsync(image, uploadPath, 5 * 1024 * 1024); // 5 MB max size
                        var imagePath = $"/uploads/products/{createdProduct.Id}/{fileName}";
                        product.Images.Add(new ProductImage { ImagePath = imagePath, IsMain = product.Images.Count == 0 });
                    }
                    
                    // Update product with images
                    await _productRepository.UpdateAsync(product);
                }

                var result = await _productRepository.GetByIdAsync(createdProduct.Id);
                return (true, null, result!.ToProductDTO());
            }
            catch (Exception ex)
            {
                return (false, $"Error creating product: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProductAsync(int id, UpdateProductDTO productDTO)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                
                if (product == null)
                    return (false, "Product not found");

                // Update basic info
                if (!string.IsNullOrEmpty(productDTO.Name))
                    product.Name = productDTO.Name;
                
                if (productDTO.Quantity.HasValue)
                    product.Quantity = productDTO.Quantity.Value;
                
                if (productDTO.ImportPrice.HasValue)
                    product.ImportPrice = productDTO.ImportPrice.Value;
                
                if (productDTO.SalePrice.HasValue)
                    product.SalePrice = productDTO.SalePrice.Value;
                
                if (!string.IsNullOrEmpty(productDTO.Description))
                    product.Description = productDTO.Description;
                
                if (productDTO.IsActive.HasValue)
                    product.IsActive = productDTO.IsActive.Value;
                
                if (productDTO.ProductLineId.HasValue)
                    product.ProductLineId = productDTO.ProductLineId.Value;

                // Update details if they exist
                if (product.Detail != null)
                {
                    if (!string.IsNullOrEmpty(productDTO.Warranty))
                        product.Detail.WarrantyMonths = int.Parse(productDTO.Warranty);
                    
                    if (!string.IsNullOrEmpty(productDTO.RAM))
                        product.Detail.RAMInGB = int.Parse(productDTO.RAM);
                    
                    if (!string.IsNullOrEmpty(productDTO.Storage))
                        product.Detail.StorageInGB = int.Parse(productDTO.Storage);
                    
                    if (!string.IsNullOrEmpty(productDTO.ScreenSize))
                        product.Detail.ScreenSizeInch = decimal.Parse(productDTO.ScreenSize);
                    
                    if (!string.IsNullOrEmpty(productDTO.ScreenResolution))
                        product.Detail.ScreenResolution = productDTO.ScreenResolution;
                    
                    if (!string.IsNullOrEmpty(productDTO.Battery))
                        product.Detail.BatteryCapacityMAh = int.Parse(productDTO.Battery);
                    
                    if (!string.IsNullOrEmpty(productDTO.Processor))
                        product.Detail.Processor = productDTO.Processor;
                    
                    if (productDTO.SimSlots.HasValue)
                        product.Detail.SimSlots = productDTO.SimSlots.Value;
                }

                // Update colors
                if (productDTO.Colors != null && productDTO.Colors.Any())
                {
                    product.Colors.Clear();
                    foreach (var colorName in productDTO.Colors)
                    {
                        product.Colors.Add(new ProductColor { Name = colorName, ProductId = product.Id });
                    }
                }

                // Add new images
                if (productDTO.AddImages != null && productDTO.AddImages.Any())
                {
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products", product.Id.ToString());
                    
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    foreach (var image in productDTO.AddImages)
                    {
                        var fileName = await ImageHelper.SaveImageAsync(image, uploadPath, 5 * 1024 * 1024); // 5 MB max size
                        var imagePath = $"/uploads/products/{product.Id}/{fileName}";
                        product.Images.Add(new ProductImage { ImagePath = imagePath, IsMain = false });
                    }
                }

                // Remove images
                if (productDTO.RemoveImagesIds != null && productDTO.RemoveImagesIds.Any())
                {
                    foreach (var imageId in productDTO.RemoveImagesIds)
                    {
                        var image = product.Images.FirstOrDefault(i => i.Id == imageId);
                        if (image != null)
                        {
                            product.Images.Remove(image);
                            // Delete physical file
                            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImagePath.TrimStart('/'));
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }
                    }
                }

                // Set main image
                if (productDTO.IdMainImage.HasValue)
                {
                    foreach (var image in product.Images)
                    {
                        image.IsMain = image.Id == productDTO.IdMainImage.Value;
                    }
                }

                product.UpdatedAt = DateTime.Now;

                var result = await _productRepository.UpdateAsync(product);
                return result ? (true, null) : (false, "Error updating product");
            }
            catch (Exception ex)
            {
                return (false, $"Error updating product: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteProductAsync(int id)
        {
            try
            {
                if (!await _productRepository.ExistsByIdAsync(id))
                    return (false, "Product not found");

                var result = await _productRepository.DeleteAsync(id);
                return result ? (true, null) : (false, "Error deleting product");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting product: {ex.Message}");
            }
        }
    }
}