using api.DTOs.Order;

namespace api.Interfaces.Services
{
    public interface IOrderService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<OrderDTO>? Orders)> GetAllOrdersAsync();
        Task<(bool Success, string? ErrorMessage, IEnumerable<OrderDTO>? Orders)> GetOrdersByUserIdAsync(Guid userId);
        Task<(bool Success, string? ErrorMessage, OrderDTO? Order)> GetOrderByIdAsync(Guid id);
        Task<(bool Success, string? ErrorMessage, OrderDTO? Order)> CreateOrderAsync(CreateOrderDTO orderDTO, Guid userId);
        Task<(bool Success, string? ErrorMessage, OrderDTO? Order)> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDTO updateOrderStatusDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteOrderAsync(Guid id);
    }
}