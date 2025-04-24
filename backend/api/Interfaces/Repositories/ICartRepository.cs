using api.Models;

namespace api.Interfaces.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetCartByUserIdAsync(Guid userId);
        Task<Cart> CreateCartAsync(Cart cart);
        Task<Cart> UpdateCartAsync(Cart cart);
        Task<bool> DeleteCartAsync(int cartId);
        Task<CartItem?> GetCartItemByIdAsync(int itemId);
        Task<CartItem> AddCartItemAsync(CartItem cartItem);
        Task<CartItem> UpdateCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(CartItem cartItem);
        Task RemoveAllCartItemsAsync(int cartId);
    }
}