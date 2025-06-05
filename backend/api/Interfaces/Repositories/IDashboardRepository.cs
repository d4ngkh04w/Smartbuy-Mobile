using api.DTOs.Dashboard;

namespace api.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<List<TopProductDTO>> GetTopProductsAsync(DateTime? startDate, DateTime? endDate, string sortBy);
        Task<OrderStatisticsDTO> GetOrderStatisticsAsync(DateTime? startDate, DateTime? endDate);
        Task<List<OrderStatusDistributionDTO>> GetOrderStatusDistributionAsync(DateTime? startDate, DateTime? endDate);
        Task<List<PaymentMethodStatDTO>> GetPaymentMethodStatsAsync(DateTime? startDate, DateTime? endDate);
        Task<List<RevenueChartDTO>> GetRevenueChartDataAsync(DateTime? startDate, DateTime? endDate, string period);
    }
}
