using api.Helpers;
using api.Interfaces.Services;
using api.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/dashboard")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "admin", Roles = "admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts([FromQuery] DashboardDateRangeQuery query)
        {
            var topProducts = await _dashboardService.GetTopProductsAsync(query);
            return ApiResponseHelper.Success("Top products retrieved successfully", topProducts);
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue([FromQuery] DashboardDateRangeQuery query)
        {
            var revenue = await _dashboardService.GetRevenueAsync(query);
            return ApiResponseHelper.Success("Revenue data retrieved successfully", revenue);
        }
    }
}
