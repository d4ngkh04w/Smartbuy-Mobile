using api.DTOs.Order;
using api.Models;

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
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                UserId = order.UserId,
                User = order.User?.ToDTO(),
                OrderItems = order.OrderItems?.Select(item => item.ToOrderItemDTO()).ToList() ?? new List<OrderItemDTO>()
            };
        }

        public static OrderItemDTO ToOrderItemDTO(this OrderItem item)
        {
            return new OrderItemDTO
            {
                Id = item.Id,
                Quantity = item.Quantity,
                Price = item.Price,
                Discount = item.Discount,
                ProductId = item.ProductId,
                Product = item.Product?.ToProductDTO()
            };
        }
    }
}