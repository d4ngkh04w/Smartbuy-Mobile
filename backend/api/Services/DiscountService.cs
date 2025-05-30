using api.DTOs.Discount;
using api.DTOs.Product;
using api.Exceptions;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly ICacheService _cacheService;

        public DiscountService(IDiscountRepository discountRepository, ICacheService cacheService)
        {
            _discountRepository = discountRepository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<DiscountDTO>> GetAllDiscountsAsync()
        {
            var discounts = await _discountRepository.GetAllDiscountsAsync();
            return discounts.Select(d => d.ToDTO());
        }

        public async Task<DiscountDTO?> GetDiscountByIdAsync(int id)
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                throw new NotFoundException($"Discount with ID {id} not found");
            }

            return discount.ToDTO();
        }

        public async Task<DiscountDTO> CreateDiscountAsync(CreateDiscountDTO discountDTO)
        {
            if (discountDTO.StartDate >= discountDTO.EndDate)
            {
                throw new BadRequestException("Start date must be earlier than end date");
            }

            // Chỉ được phép discount amount hoặc discount percentage
            if (discountDTO.DiscountAmount > 0 && discountDTO.DiscountPercentage > 0)
            {
                throw new BadRequestException("Only one of DiscountAmount or DiscountPercentage can be set");
            }

            var createdDiscount = await _discountRepository.CreateDiscountAsync(discountDTO.ToModel());
            return createdDiscount.ToDTO();
        }

        public async Task<DiscountDTO?> UpdateDiscountAsync(int id, UpdateDiscountDTO discountDTO)
        {
            var existingDiscount = await _discountRepository.GetDiscountByIdAsync(id);
            if (existingDiscount == null)
            {
                throw new NotFoundException($"Discount with ID {id} not found");
            }
            if (discountDTO.DiscountPercentage.HasValue)
            {
                existingDiscount.DiscountPercentage = discountDTO.DiscountPercentage.Value;
            }

            if (discountDTO.DiscountAmount.HasValue)
            {
                existingDiscount.DiscountAmount = discountDTO.DiscountAmount.Value;
            }

            if (discountDTO.StartDate.HasValue)
            {
                existingDiscount.StartDate = discountDTO.StartDate.Value;
            }

            if (discountDTO.EndDate.HasValue)
            {
                existingDiscount.EndDate = discountDTO.EndDate.Value;
            }

            // Validate dates after update
            if (existingDiscount.StartDate >= existingDiscount.EndDate)
            {
                throw new BadRequestException("Start date must be earlier than end date");
            }

            _cacheService.RemoveAllProductsCache();
            foreach (var product in existingDiscount.Products)
            {
                _cacheService.RemoveProductCache(product.ProductId);
            }

            var updatedDiscount = await _discountRepository.UpdateDiscountAsync(id, existingDiscount);
            return updatedDiscount?.ToDTO();
        }

        public async Task<bool> DeleteDiscountAsync(int id)
        {
            var discount = await _discountRepository.GetDiscountByIdAsync(id);
            if (discount == null)
            {
                throw new NotFoundException($"Discount with ID {id} not found");
            }

            _cacheService.RemoveAllProductsCache();
            foreach (var product in discount.Products)
            {
                _cacheService.RemoveProductCache(product.ProductId);
            }

            return await _discountRepository.DeleteDiscountAsync(id);
        }

        public async Task<bool> AddProductsToDiscountAsync(int discountId, AddProductToDiscountDTO productsDTO)
        {
            var exists = await _discountRepository.IsDiscountExistAsync(discountId);
            if (!exists)
            {
                throw new NotFoundException($"Discount with ID {discountId} not found");
            }

            if (productsDTO.ProductIds.Count == 0)
            {
                throw new BadRequestException("At least one product ID is required");
            }

            var results = new List<bool>();
            foreach (var productId in productsDTO.ProductIds)
            {
                var result = await _discountRepository.AddProductToDiscountAsync(discountId, productId);
                results.Add(result);
                _cacheService.RemoveProductCache(productId);
            }

            _cacheService.RemoveAllProductsCache();

            return results.Any(r => r);
        }

        public async Task<bool> RemoveProductFromDiscountAsync(int discountId, int productId)
        {
            var exists = await _discountRepository.IsDiscountExistAsync(discountId);
            if (!exists)
            {
                throw new NotFoundException($"Discount with ID {discountId} not found");
            }

            _cacheService.RemoveProductCache(productId);
            _cacheService.RemoveAllProductsCache();

            return await _discountRepository.RemoveProductFromDiscountAsync(discountId, productId);
        }

        public async Task<IEnumerable<ProductDTO>> GetProductsByDiscountIdAsync(int discountId)
        {
            var products = await _discountRepository.GetProductsByDiscountIdAsync(discountId);
            return products.Select(p => p.ToProductDTO());
        }

        public async Task<IEnumerable<DiscountDTO>> GetDiscountsByProductIdAsync(int productId)
        {
            var discounts = await _discountRepository.GetDiscountsByProductIdAsync(productId);
            return discounts.Select(d => d.ToDTO());
        }
    }
}
