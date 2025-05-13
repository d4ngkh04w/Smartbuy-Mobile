using api.DTOs.Order;
using api.Models;
using api.Utils;

namespace api.Mappers
{
    public static class OrderMapper
    {
        public static OrderDTO ToOrderDTO(this Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                ShippingFee = order.ShippingFee,
                OrderDate = DateTimeUtils.FormatDateTime(order.OrderDate),
                DeliveryDate = order.DeliveryDate.HasValue ? DateTimeUtils.FormatDateTime(order.DeliveryDate.Value) : null,
                UserId = order.UserId,
                User = order.User?.ToDTO(),
                OrderItems = order.OrderItems?.Select(item => item.ToOrderItemDTO()).ToList() ?? new List<OrderItemDTO>()
            };
        }

        public static OrderItemDTO ToOrderItemDTO(this OrderItem item)
        {
            var orderItemDto = new OrderItemDTO
            {
                Id = item.Id,
                Quantity = item.Quantity,
                Price = item.Price,
                Discount = item.Discount,
                ProductId = item.ProductId,
                Product = item.Product?.ToProductDTO(),
                ColorId = item.ColorId
            };

            // Set color information if available
            if (item.Color != null)
            {
                orderItemDto.ColorName = item.Color.Name;

                // Get the first image from the color, if there's any
                var firstImage = item.Color.Images.FirstOrDefault();
                if (firstImage != null)
                {
                    orderItemDto.ColorImage = firstImage.ImagePath;
                }
            }

            return orderItemDto;
        }
    }
}