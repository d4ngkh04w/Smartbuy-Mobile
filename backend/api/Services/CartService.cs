using api.DTOs.Cart;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, CartDTO? Cart)> GetCartAsync(Guid userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);

                // Nếu giỏ hàng không tồn tại, tạo một giỏ hàng mới
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>()
                    };

                    await _cartRepository.CreateCartAsync(cart);
                }

                return (true, null, cart.ToCartDTO());
            }
            catch (Exception)
            {
                return (false, $"Error retrieving cart", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CartDTO? Cart)> AddToCartAsync(Guid userId, AddToCartDTO dto)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(dto.ProductId);
                if (product == null)
                    return (false, "Product not found", null);

                if (!product.IsActive)
                    return (false, "Product is no longer available", null);

                if (product.Quantity < dto.Quantity)
                    return (false, "Not enough product in stock", null);

                // Lấy giỏ hàng của người dùng hoặc tạo mới nếu không tồn tại
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        Items = new List<CartItem>()
                    };

                    cart = await _cartRepository.CreateCartAsync(cart);
                }

                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                var existingItem = cart.Items?.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
                if (existingItem != null)
                {
                    // Cập nhật số lượng sản phẩm trong giỏ hàng
                    existingItem.Quantity += dto.Quantity;

                    // Nếu số lượng sản phẩm trong giỏ hàng lớn hơn số lượng có sẵn, đặt lại số lượng sản phẩm trong giỏ hàng về số lượng có sẵn
                    if (existingItem.Quantity > product.Quantity)
                        existingItem.Quantity = product.Quantity;

                    await _cartRepository.UpdateCartItemAsync(existingItem);
                }
                else
                {
                    // Thêm mới sản phẩm vào giỏ hàng
                    var newItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity
                    };

                    await _cartRepository.AddCartItemAsync(newItem);
                }

                // Lấy giỏ hàng đã cập nhật với thông tin sản phẩm
                var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
                return (true, null, updatedCart?.ToCartDTO());
            }
            catch (Exception)
            {
                return (false, $"Error adding to cart", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CartDTO? Cart)> UpdateCartItemAsync(Guid userId, Guid itemId, UpdateCartItemDTO dto)
        {
            try
            {
                // Lấy những sản phẩm trong giỏ hàng
                var cartItem = await _cartRepository.GetCartItemByIdAsync(itemId);
                if (cartItem == null)
                    return (false, "Cart item not found", null);

                // Lấy giỏ hàng để xác minh quyền sở hữu
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null || cart.Id != cartItem.CartId)
                    return (false, "Cart item does not belong to user", null);

                // Kiểm tra xem sản phẩm có tồn tại không
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                    return (false, "Product no longer exists", null);

                // Kiểm tra xem sản phẩm có còn khả dụng không
                if (!product.IsActive)
                    return (false, "Product is no longer available", null);

                // Kiểm tra xem có đủ hàng trong kho không
                if (product.Quantity < dto.Quantity)
                {
                    // Đặt số lượng sản phẩm trong giỏ hàng về số lượng tối đa có sẵn
                    cartItem.Quantity = product.Quantity;
                    await _cartRepository.UpdateCartItemAsync(cartItem);

                    var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
                    return (false, "Quantity adjusted to maximum available stock", updatedCart?.ToCartDTO());
                }

                // Cập nhật số lượng sản phẩm trong giỏ hàng
                cartItem.Quantity = dto.Quantity;
                await _cartRepository.UpdateCartItemAsync(cartItem);

                // Trả về giỏ hàng đã cập nhật với thông tin sản phẩm
                var result = await _cartRepository.GetCartByUserIdAsync(userId);
                return (true, null, result?.ToCartDTO());
            }
            catch (Exception)
            {
                return (false, $"Error updating cart item", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveCartItemAsync(Guid userId, Guid itemId)
        {
            try
            {
                var cartItem = await _cartRepository.GetCartItemByIdAsync(itemId);
                if (cartItem == null)
                    return (false, "Cart item not found");

                // Lấy giỏ hàng để xác minh quyền sở hữu
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null || cart.Id != cartItem.CartId)
                    return (false, "Cart item does not belong to user");

                // Xóa sản phẩm khỏi giỏ hàng
                await _cartRepository.RemoveCartItemAsync(cartItem);

                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error removing cart item");
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ClearCartAsync(Guid userId)
        {
            try
            {
                // Lấy giỏ hàng của người dùng
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null)
                    // Không có giỏ hàng, không có gì để xóa
                    return (true, null);

                // Xóa tất cả các sản phẩm trong giỏ hàng
                await _cartRepository.RemoveAllCartItemsAsync(cart.Id);

                return (true, null);
            }
            catch (Exception)
            {
                return (false, $"Error clearing cart");
            }
        }
    }
}