using api.DTOs.Cart;
using api.Mappers;
using api.Models;

namespace api.Mappers
{
    public static class CartMapper
    {
        public static CartDTO ToCartDTO(this Cart cart)
        {
            var cartDto = new CartDTO
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cart.Items?.Select(ci => ci.ToCartItemDTO()) ?? new List<CartItemDTO>(),
            };

            // Calculate total price based on all item subtotals
            cartDto.TotalPrice = cartDto.CartItems.Sum(item => item.SubTotal);

            return cartDto;
        }

        public static CartItemDTO ToCartItemDTO(this CartItem cartItem)
        {
            var cartItemDto = new CartItemDTO
            {
                Id = cartItem.Id,
                Quantity = cartItem.Quantity,
                ProductId = cartItem.ProductId,
                Product = cartItem.Product?.ToProductDTO()
            };

            // Calculate subtotal based on product price and quantity
            cartItemDto.SubTotal = cartItem.Product != null
                ? cartItem.Product.SalePrice * cartItem.Quantity
                : 0;

            return cartItemDto;
        }
    }
}