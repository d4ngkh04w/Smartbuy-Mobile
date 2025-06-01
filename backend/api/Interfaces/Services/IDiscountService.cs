using api.DTOs.Discount;
using api.DTOs.Product;


namespace api.Interfaces.Services
{
    public interface IDiscountService
    {
        Task<IEnumerable<DiscountDTO>> GetAllDiscountsAsync();
        Task<DiscountDTO?> GetDiscountByIdAsync(int id);
        Task<DiscountDTO> CreateDiscountAsync(CreateDiscountDTO discountDTO);
        Task<DiscountDTO?> UpdateDiscountAsync(int id, UpdateDiscountDTO discountDTO);
        Task<bool> DeleteDiscountAsync(int id);
        Task<bool> AddProductsToDiscountAsync(int discountId, AddProductToDiscountDTO productIds);
        Task<bool> RemoveProductFromDiscountAsync(int discountId, int productId);
        Task<IEnumerable<ProductDTO>> GetProductsByDiscountIdAsync(int discountId);
        Task<IEnumerable<DiscountDTO>> GetDiscountsByProductIdAsync(int productId);
    }
}
