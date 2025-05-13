using api.DTOs.Dashboard;
using api.Queries;

namespace api.Interfaces.Services
{
    public interface IDashboardService
    {
        Task<List<TopProductDTO>> GetTopProductsAsync(DashboardDateRangeQuery query);
        Task<List<RevenueDTO>> GetRevenueAsync(DashboardDateRangeQuery query);
    }
}
