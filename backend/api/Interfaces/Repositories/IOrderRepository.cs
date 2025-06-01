using api.Models;

namespace api.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
        Task<IEnumerable<Order>> GetCurrentOrdersByUserIdAsync(Guid userId);
        Task<Order?> GetOrderByIdAsync(Guid id);
        Task<Order> CreateOrderAsync(Order order);
        Task<Order> UpdateOrderAsync(Order order);
<<<<<<< HEAD
        Task<bool> DeleteOrderAsync(Guid id);

=======
>>>>>>> b8f062a0cf29a17c478140a7df914fb668c43dd5
    }
}