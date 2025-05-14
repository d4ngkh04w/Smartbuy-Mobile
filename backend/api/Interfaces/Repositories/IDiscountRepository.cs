using api.Models;

namespace api.Interfaces.Repositories
{
    public interface IDiscountRepository
    {
        Task<IEnumerable<Discount>> GetAllDiscountsAsync();
        Task<Discount?> GetDiscountByIdAsync(int id);
        Task<Discount> CreateDiscountAsync(Discount discount);
        Task<Discount?> UpdateDiscountAsync(int id, Discount discount);
        Task<bool> DeleteDiscountAsync(int id);
        Task<bool> IsDiscountExistAsync(int id);
        Task<bool> AddProductToDiscountAsync(int discountId, int productId);
        Task<bool> RemoveProductFromDiscountAsync(int discountId, int productId);
        Task<IEnumerable<Product>> GetProductsByDiscountIdAsync(int discountId);
        Task<IEnumerable<Discount>> GetDiscountsByProductIdAsync(int productId);
    }
}
