using api.DTOs.Cart;

namespace api.Interfaces.Services
{
    public interface ICartService
    {
        Task<(bool Success, string? ErrorMessage, CartDTO? Cart)> GetCartAsync(Guid userId);
        Task<(bool Success, string? ErrorMessage, CartDTO? Cart)> AddToCartAsync(Guid userId, AddToCartDTO dto);
        Task<(bool Success, string? ErrorMessage, CartDTO? Cart)> UpdateCartItemAsync(Guid userId, Guid itemId, UpdateCartItemDTO dto);
        Task<(bool Success, string? ErrorMessage)> RemoveCartItemAsync(Guid userId, Guid itemId);
        Task<(bool Success, string? ErrorMessage)> ClearCartAsync(Guid userId);
    }
}