using api.DTOs.Cart;
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
                ColorId = cartItem.ColorId,
                Product = cartItem.Product?.ToProductDTO()
            };

            if (cartItem.Color != null)
            {
                cartItemDto.ColorName = cartItem.Color.Name;

                var firstImage = cartItem.Color.Images.FirstOrDefault();
                if (firstImage != null)
                {
                    cartItemDto.ColorImage = firstImage.ImagePath;
                }            }            cartItemDto.SubTotal = cartItem.Product != null
                ? CalculateDiscountedPrice(cartItem.Product) * cartItem.Quantity
                : 0;

            return cartItemDto;
        }

        // Hàm tính giá sau khi giảm giá (tương tự logic trong ProductMapper)
        private static decimal CalculateDiscountedPrice(Product product)
        {
            if (product.Discounts == null || !product.Discounts.Any())
                return product.SalePrice;

            return product.Discounts
                .Select(d => d.Discount != null 
                    ? d.Discount.CalculateDiscountedPrice(product.SalePrice) 
                    : product.SalePrice)
                .DefaultIfEmpty(product.SalePrice)
                .Min();
        }
    }
}