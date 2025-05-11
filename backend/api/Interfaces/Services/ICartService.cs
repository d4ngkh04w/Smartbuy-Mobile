using api.DTOs.Cart;

namespace api.Interfaces.Services
{
    public interface ICartService
    {
        Task<CartDTO> GetCartAsync(Guid userId);
        Task<CartDTO> AddToCartAsync(Guid userId, AddToCartDTO dto);
        Task<CartDTO> UpdateCartItemAsync(Guid userId, Guid itemId, UpdateCartItemDTO dto);
        Task RemoveCartItemAsync(Guid userId, Guid itemId);
        Task ClearCartAsync(Guid userId);
    }
}