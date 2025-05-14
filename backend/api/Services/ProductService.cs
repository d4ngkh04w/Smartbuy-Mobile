using api.DTOs.Product;
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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductLineRepository _productLineRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IWebHostEnvironment _env;
        private readonly ICacheService _cacheService;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _pagedCacheDuration = TimeSpan.FromMinutes(5);
        public ProductService(IProductRepository productRepository,
                              IProductLineRepository productLineRepository,
                              ICommentRepository commentRepository,
                              IWebHostEnvironment webHostEnvironment,
                              ICacheService cacheService)
        {
            _productRepository = productRepository;
            _productLineRepository = productLineRepository;
            _commentRepository = commentRepository;
            _env = webHostEnvironment;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsAsync()
        {
            string cacheKey = CacheKeyManager.GetAllProductsKey();

            if (_cacheService.TryGetValue(cacheKey, out IEnumerable<ProductDTO>? cachedProducts) && cachedProducts != null)
            {
                return cachedProducts;
            }

            var products = await _productRepository.GetAllAsync();

            if (!products.Any())
                return new List<ProductDTO>();

            var productDtos = (await Task.WhenAll(products.Select(async p => await p.ToProductDTO(_commentRepository)))).ToList();

            _cacheService.Set(cacheKey, productDtos, _cacheDuration);

            return productDtos;
        }

        public async Task<ProductDTO> GetProductByIdAsync(int id)
        {
            string cacheKey = CacheKeyManager.GetProductKey(id);

            if (_cacheService.TryGetValue(cacheKey, out ProductDTO? cachedProduct) && cachedProduct != null)
            {
                return cachedProduct;
            }

            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            var productDto = await product.ToProductDTO(_commentRepository);

            _cacheService.Set(cacheKey, productDto, _cacheDuration);

            return productDto;
        }

        public async Task<ProductDTO> CreateProductAsync(CreateProductDTO productDTO)
        {
            if (await _productRepository.ExistsByNameAsync(productDTO.Name.Trim()))
                throw new AlreadyExistsException("Product with this name already exists");
            else
                productDTO.Name = productDTO.Name.Trim();

            var productLine = await _productLineRepository.GetProductLineByIdAsync(productDTO.ProductLineId) ?? throw new NotFoundException($"Product line with ID {productDTO.ProductLineId} not found");
            if (!productLine.IsActive)
                throw new BadRequestException($"Product line with ID {productDTO.ProductLineId} is inactive");

            var product = productDTO.ToProductModel();
            product.Detail = productDTO.ToProductDetailModel();

            var createdProduct = await _productRepository.CreateAsync(product);            // Xóa cache danh sách sản phẩm
            _cacheService.RemoveAllProductsCache();

            return await createdProduct.ToProductDTO(_commentRepository);
        }

        public async Task<ProductDTO> UpdateProductAsync(int id, UpdateProductDTO productDTO)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");

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

            var updatedProduct = await _productRepository.UpdateAsync(product);
            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();
            return await updatedProduct.ToProductDTO(_commentRepository);
        }

        public async Task<ProductColorDTO> UpdateProductColorAsync(int productId, int colorId, UpdateColorDTO productColorDTO)
        {
            if (productColorDTO.AddImages != null)
            {
                foreach (var image in productColorDTO.AddImages)
                {
                    if (!image.IsImage())
                        throw new BadRequestException("Invalid image format");
                }
            }

            _ = await _productRepository.GetByIdAsync(productId) ?? throw new NotFoundException("Product not found");
            var productColor = await _productRepository.GetProductColorAsync(productId, colorId) ?? throw new NotFoundException("Product color not found");

            if (!string.IsNullOrEmpty(productColorDTO.Name?.Trim()))
            {
                if (productColor.Name == productColorDTO.Name.Trim())
                    throw new AlreadyExistsException($"Color with name {productColorDTO.Name.Trim()} already exists for this product");

                productColor.Name = productColorDTO.Name.Trim();
            }

            if (productColorDTO.Quantity.HasValue)
                productColor.Quantity = productColorDTO.Quantity.Value;

            if (productColorDTO.RemoveImageIds != null && productColorDTO.RemoveImageIds.Any())
            {
                foreach (var imageId in productColorDTO.RemoveImageIds)
                {
                    var image = productColor.Images.FirstOrDefault(i => i.Id == imageId);
                    if (image != null)
                    {
                        var deletedImg = ImageUtils.DeleteImage(_env.WebRootPath + image.ImagePath);
                        if (!deletedImg)
                        {
                            Console.WriteLine($"[WARN]: Failed to delete old image file: {image.ImagePath}");
                        }
                        productColor.Images.Remove(image);
                    }
                }
            }

            if (productColorDTO.AddImages != null && productColorDTO.AddImages.Any())
            {
                for (int i = 0; i < productColorDTO.AddImages.Count; i++)
                {
                    var image = productColorDTO.AddImages[i];
                    var filePath = await ImageUtils.SaveImageAsync(image, _env.WebRootPath, "products", 5 * 1024 * 1024); // 5 MB max size

                    if (productColorDTO.MainImageIndex.HasValue && productColorDTO.MainImageIndex.Value == i)
                    {
                        var mainImage = productColor.Images.Where(i => i.IsMain);
                        foreach (var img in mainImage)
                        {
                            img.IsMain = false;
                        }
                    }

                    var productImage = new ProductImage
                    {
                        ImagePath = filePath,
                        IsMain = productColorDTO.MainImageIndex.HasValue && productColorDTO.MainImageIndex.Value == i,
                        ColorId = productColor.Id
                    };
                    productColor.Images.Add(productImage);
                }
            }

            var updatedColor = await _productRepository.UpdateColorAsync(productColor);
            _cacheService.RemoveProductCache(productId);
            _cacheService.RemoveAllProductsCache();
            return updatedColor.ToProductColorDTO();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            await _productRepository.DeleteAsync(product);
            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();
        }
        public async Task<ProductPagiDTO> GetPagedProductsAsync(ProductQuery productQuery)
        {
            string cacheKey = CacheKeyManager.GetPagedProductsKey(productQuery);

            if (_cacheService.TryGetValue(cacheKey, out ProductPagiDTO? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            var (items, totalItems) = await _productRepository.GetPagedProductsAsync(productQuery);

            if (items == null || !items.Any())
            {
                return new ProductPagiDTO
                {
                    TotalItems = 0,
                    Items = new List<ProductSummaryDTO>()
                };
            }

            var productSummaries = await Task.WhenAll(items.Select(async p => await p.ToSummaryDTO(_commentRepository)));

            var result = new ProductPagiDTO
            {
                TotalItems = totalItems,
                Items = productSummaries.ToList()
            };
            _cacheService.Set(cacheKey, result, _pagedCacheDuration);
            return result;
        }

        public async Task<ProductColorDTO> CreateProductColorAsync(int productId, CreateColorDTO productColorDTO)
        {
            foreach (var image in productColorDTO.Images)
            {
                if (!image.IsImage())
                    throw new BadRequestException("Invalid image format");
            }
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

            _cacheService.RemoveProductCache(productId);
            _cacheService.RemoveAllProductsCache();

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

            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();

            return await result.ToProductDTO(_commentRepository);
        }

        public async Task<ProductDTO> DeactivateProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id) ?? throw new NotFoundException("Product not found");
            if (!product.IsActive)
                return await product.ToProductDTO(_commentRepository);

            product.IsActive = false;
            product.ManuallyDeactivated = true;
            product.UpdatedAt = DateTime.Now;

            var result = await _productRepository.UpdateAsync(product);

            _cacheService.RemoveProductCache(id);
            _cacheService.RemoveAllProductsCache();

            return result.ToProductDTO();
        }
    }
}