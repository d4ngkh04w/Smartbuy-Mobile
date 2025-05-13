using api.DTOs.Dashboard;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Queries;

namespace api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<List<TopProductDTO>> GetTopProductsAsync(DashboardDateRangeQuery query)
        {
            var data = await _dashboardRepository.GetTopProductsAsync(
                query.StartDate,
                query.EndDate,
                query.SortBy ?? "quantity"
            );

            return data;
        }

        public async Task<List<RevenueDTO>> GetRevenueAsync(DashboardDateRangeQuery query)
        {
            var data = await _dashboardRepository.GetRevenueAsync(
                query.StartDate,
                query.EndDate,
                query.Period ?? "month"
            );

            return data;
        }
    }
}
