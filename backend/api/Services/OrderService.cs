using api.DTOs.Order;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;

namespace api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICartRepository _cartRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICartRepository cartRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _cartRepository = cartRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, OrderDTO? Order)> CreateOrderAsync(CreateOrderDTO orderDTO, Guid userId)
        {
            try
            {
                Console.WriteLine("Test");
                if (orderDTO.Items == null || !orderDTO.Items.Any())
                {
                    return (false, "Order must contain at least one item", null);
                }

                // Tạo order mới
                var order = new Order
                {
                    UserId = userId,
                    Status = "Chờ xác nhận",
                    PaymentMethod = orderDTO.PaymentMethod,
                    OrderDate = DateTime.Now,
                    ShippingFee = orderDTO.ShippingFee,
                    OrderItems = new List<OrderItem>()
                };

                Console.WriteLine("Test");

                decimal totalAmount = 0;

                foreach (var item in orderDTO.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                        return (false, $"Product with ID {item.ProductId} not found", null);

                    if (!product.IsActive)
                        return (false, $"Product '{product.Name}' is no longer available", null);

                    if (product.Quantity < item.Quantity)
                        return (false, $"Not enough quantity available for product '{product.Name}'", null);

                    // Tạo order item
                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = product.SalePrice,
                        Discount = 0 // Update later
                    };

                    // Update total
                    totalAmount += orderItem.Price * orderItem.Quantity;

                    // Add item vào order
                    order.OrderItems.Add(orderItem);

                    // Update số lượng sản phẩm
                    product.Quantity -= item.Quantity;
                    product.Sold += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                    Console.WriteLine("Test1");
                }

                Console.WriteLine("Test");

                // Set tổng tiền cho order
                order.TotalAmount = totalAmount + orderDTO.ShippingFee;

                // Lưu order
                Console.WriteLine("Test2");
                var createdOrder = await _orderRepository.CreateOrderAsync(order);
                Console.WriteLine("Test3");
                // Xóa item trong giỏ hàng của người dùng sau khi đặt hàng thông qua giỏ hàng
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart != null)
                {
                    await _cartRepository.RemoveAllCartItemsAsync(cart.Id);
                }

                return (true, null, createdOrder.ToOrderDTO());
            }
            catch (Exception)
            {
                return (false, "Error creating order", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteOrderAsync(Guid id)
        {
            try
            {
                var result = await _orderRepository.DeleteOrderAsync(id);
                if (!result)
                {
                    return (false, "Order not found");
                }

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "Error deleting order");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<OrderDTO>? Orders)> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersAsync();
                if (orders == null || !orders.Any())
                {
                    return (false, "Not found any orders", null);
                }

                var orderDTOs = orders.Select(o => o.ToOrderDTO()).ToList();
                return (true, null, orderDTOs);
            }
            catch (Exception)
            {
                return (false, "Error retrieving orders", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, OrderDTO? Order)> GetOrderByIdAsync(Guid id)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return (false, "Order not found", null);
                }

                return (true, null, order.ToOrderDTO());
            }
            catch (Exception)
            {
                return (false, "Error retrieving order", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<OrderDTO>? Orders)> GetOrdersByUserIdAsync(Guid userId)
        {
            try
            {
                var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
                if (orders == null || !orders.Any())
                {
                    return (false, "Not found any orders for you", null);
                }

                var orderDTOs = orders.Select(o => o.ToOrderDTO()).ToList();
                return (true, null, orderDTOs);
            }
            catch (Exception)
            {
                return (false, "Error retrieving user's orders", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, OrderDTO? Order)> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return (false, "Order not found", null);
                }

                // Kiểm tra trạng thái hiện tại và trạng thái mới có hợp lệ không
                if (!IsValidStatusTransition(order.Status, updateOrderStatusDTO.Status))
                {
                    return (false, $"Invalid status transition from '{order.Status}' to '{updateOrderStatusDTO.Status}'", null);
                }

                // Update order status
                order.Status = updateOrderStatusDTO.Status;

                if (updateOrderStatusDTO.Status == "Đã giao hàng")
                {
                    order.DeliveryDate = updateOrderStatusDTO.DeliveryDate ?? DateTime.Now;
                }

                var result = await _orderRepository.UpdateOrderAsync(order);
                if (!result)
                {
                    return (false, "Failed to update order status", null);
                }

                return (true, null, order.ToOrderDTO());
            }
            catch (Exception)
            {
                return (false, "Error updating order status", null);
            }
        }

        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            Dictionary<string, List<string>> validTransitions = new Dictionary<string, List<string>>
            {
                ["Chờ xác nhận"] = new List<string> { "Đã xác nhận", "Đã huỷ" },
                ["Đã xác nhận"] = new List<string> { "Đang giao hàng", "Đã huỷ" },
                ["Đang giao hàng"] = new List<string> { "Đã giao hàng", "Đã huỷ" },
                ["Đã giao hàng"] = new List<string> { "Đã hoàn tiền", "Đã trả hàng" },
                ["Đã huỷ"] = new List<string>(),
                ["Đã hoàn tiền"] = new List<string>(),
                ["Đã trả hàng"] = new List<string>()
            };

            // Kiểm tra chuyển trạng thái hợp lệ
            if (validTransitions.ContainsKey(currentStatus) && validTransitions[currentStatus].Contains(newStatus))
            {
                return true;
            }

            // Cho phép chuyển trạng thái bất kỳ nếu trạng thái hiện tại không được định nghĩa
            if (!validTransitions.ContainsKey(currentStatus))
            {
                return true;
            }

            return false;
        }
    }
}