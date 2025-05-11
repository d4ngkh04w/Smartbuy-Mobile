using api.DTOs.Cart;
using api.Exceptions;
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

        public async Task<CartDTO> GetCartAsync(Guid userId)
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

            return cart.ToCartDTO();
        }

        public async Task<CartDTO> AddToCartAsync(Guid userId, AddToCartDTO dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId) ?? throw new NotFoundException("Product not found");

            if (!product.IsActive)
                throw new BadRequestException("Product is no longer available");

            // Find the specific color
            var color = product.Colors.FirstOrDefault(c => c.Id == dto.ColorId) ?? throw new BadRequestException("Selected color not found");

            // Check if there's enough quantity for the selected color
            if (color.Quantity < dto.Quantity)
                throw new BadRequestException("Not enough product in stock for the selected color");

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

            // Kiểm tra xem sản phẩm với màu sắc này đã có trong giỏ hàng chưa
            var existingItem = cart.Items?.FirstOrDefault(ci =>
                ci.ProductId == dto.ProductId &&
                ci.ColorId == dto.ColorId);

            if (existingItem != null)
            {
                // Cập nhật số lượng sản phẩm trong giỏ hàng
                existingItem.Quantity += dto.Quantity;

                // Nếu số lượng sản phẩm trong giỏ hàng lớn hơn số lượng có sẵn, đặt lại số lượng sản phẩm trong giỏ hàng về số lượng có sẵn
                if (existingItem.Quantity > color.Quantity)
                    existingItem.Quantity = color.Quantity;

                await _cartRepository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                // Thêm mới sản phẩm vào giỏ hàng
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,
                    ColorId = dto.ColorId,
                    Quantity = dto.Quantity
                };

                await _cartRepository.AddCartItemAsync(newItem);
            }

            // Lấy giỏ hàng đã cập nhật với thông tin sản phẩm
            var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
            return updatedCart!.ToCartDTO();
        }

        public async Task<CartDTO> UpdateCartItemAsync(Guid userId, Guid itemId, UpdateCartItemDTO dto)
        {
            // Lấy những sản phẩm trong giỏ hàng
            var cartItem = await _cartRepository.GetCartItemByIdAsync(itemId) ?? throw new NotFoundException("Cart item not found");

            // Lấy giỏ hàng để xác minh quyền sở hữu
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Id != cartItem.CartId)
                throw new ForbiddenException("Cart item does not belong to user");

            // Kiểm tra xem sản phẩm có tồn tại không
            var product = await _productRepository.GetByIdAsync(cartItem.ProductId) ?? throw new NotFoundException("Product no longer exists");

            // Kiểm tra xem sản phẩm có còn khả dụng không
            if (!product.IsActive)
                throw new NotFoundException("Product is no longer available");

            // Find the specific color
            var color = product.Colors.FirstOrDefault(c => c.Id == cartItem.ColorId) ?? throw new NotFoundException("Selected color no longer available");

            // Kiểm tra xem có đủ hàng trong kho không
            if (color.Quantity < dto.Quantity)
            {
                // Đặt số lượng sản phẩm trong giỏ hàng về số lượng tối đa có sẵn
                cartItem.Quantity = color.Quantity;
                await _cartRepository.UpdateCartItemAsync(cartItem);

                var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);
                return updatedCart!.ToCartDTO();
            }

            // Cập nhật số lượng sản phẩm trong giỏ hàng
            cartItem.Quantity = dto.Quantity;
            await _cartRepository.UpdateCartItemAsync(cartItem);

            // Trả về giỏ hàng đã cập nhật với thông tin sản phẩm
            var result = await _cartRepository.GetCartByUserIdAsync(userId);
            return result!.ToCartDTO();
        }

        public async Task RemoveCartItemAsync(Guid userId, Guid itemId)
        {
            var cartItem = await _cartRepository.GetCartItemByIdAsync(itemId) ?? throw new NotFoundException("Cart item not found");

            // Lấy giỏ hàng để xác minh quyền sở hữu
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null || cart.Id != cartItem.CartId)
                throw new ForbiddenException("Cart item does not belong to user");

            // Xóa sản phẩm khỏi giỏ hàng
            await _cartRepository.RemoveCartItemAsync(cartItem);
        }

        public async Task ClearCartAsync(Guid userId)
        {
            // Lấy giỏ hàng của người dùng
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
                return;

            // Xóa tất cả các sản phẩm trong giỏ hàng
            await _cartRepository.RemoveAllCartItemsAsync(cart.Id);
        }
    }
}