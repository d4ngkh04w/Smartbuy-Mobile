using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDBContext _context;

        public CartRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems!)
                    .ThenInclude(ci => ci.Product!)
                        .ThenInclude(p => p.Images!)
                .Include(c => c.CartItems!)
                    .ThenInclude(ci => ci.Product!)
                        .ThenInclude(p => p.Colors!)
                .FirstOrDefaultAsync(c => c.UserId.ToString() == userId.ToString());
        }

        public async Task<Cart> CreateCartAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<Cart> UpdateCartAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task<bool> DeleteCartAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart == null)
                return false;

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int itemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product!)
                    .ThenInclude(p => p.Images)
                .Include(ci => ci.Product!)
                    .ThenInclude(p => p.Colors)
                .FirstOrDefaultAsync(ci => ci.Id == itemId);
        }

        public async Task<CartItem> AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task<CartItem> UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
            return cartItem;
        }

        public async Task RemoveCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAllCartItemsAsync(int cartId)
        {
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();

            if (!cartItems.Any())
                return;

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
        }
    }
}