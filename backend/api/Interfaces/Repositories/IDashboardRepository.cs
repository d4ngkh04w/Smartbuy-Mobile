using api.DTOs.Dashboard;

namespace api.Interfaces.Repositories
{
    public interface IDashboardRepository
    {
        Task<List<TopProductDTO>> GetTopProductsAsync(DateTime? startDate, DateTime? endDate, string sortBy);
        Task<List<RevenueDTO>> GetRevenueAsync(DateTime? startDate, DateTime? endDate, string period);
    }
}
