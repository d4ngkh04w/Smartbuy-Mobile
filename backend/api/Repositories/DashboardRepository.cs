using api.Database;
using api.DTOs.Dashboard;
using api.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDBContext _db;

        public DashboardRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<List<TopProductDTO>> GetTopProductsAsync(DateTime? startDate, DateTime? endDate, string sortBy)
        {
            var query = _db.Orders
                .Where(o => o.Status == "Hoàn thành");

            if (startDate.HasValue)
            {
                query = query.Where(o => o.DeliveryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.DeliveryDate <= endDate.Value);
            }

            // Group by product and sum quantities and revenues
            var result = await query
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                .Select(g => new TopProductDTO
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Quantity = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Price * oi.Quantity * (1 - oi.Discount / 100)),
                    CreatedAt = g.First().Product!.CreatedAt
                })
                .ToListAsync();
            foreach (var product in result)
            {
                product.CreatedAtFormatted = product.CreatedAt.ToString("yyyy-MM-dd");
            }

            // Sort the results based on the sortBy parameter
            return sortBy.ToLower() == "revenue"
                ? result.OrderByDescending(p => p.Revenue).ToList()
                : result.OrderByDescending(p => p.Quantity).ToList();
        }

        public async Task<List<RevenueDTO>> GetRevenueAsync(DateTime? startDate, DateTime? endDate, string period)
        {            // Start with all completed orders
            var completedOrders = _db.Orders
                .Where(o => o.Status == "Hoàn thành");

            // Apply date filters if provided
            if (startDate.HasValue)
            {
                completedOrders = completedOrders.Where(o => o.DeliveryDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                completedOrders = completedOrders.Where(o => o.DeliveryDate <= endDate.Value);
            }

            // Ensure we're only working with orders that have a delivery date
            completedOrders = completedOrders.Where(o => o.DeliveryDate.HasValue);

            switch (period.ToLower())
            {
                case "day":
                    // For daily grouping - using a client-side evaluation approach to avoid EF Core translation issues
                    var dailyResults = await completedOrders
                        .GroupBy(o => o.DeliveryDate!.Value.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Amount = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Date)
                        .ToListAsync();

                    return dailyResults.Select(r => new RevenueDTO
                    {
                        Date = r.Date,
                        DisplayDate = r.Date.ToString("yyyy-MM-dd"),
                        Amount = r.Amount,
                        OrderCount = r.OrderCount
                    }).ToList();

                case "month":
                    // For monthly grouping - using a client-side evaluation approach to avoid EF Core translation issues
                    var monthlyResults = await completedOrders
                        .GroupBy(o => new { o.DeliveryDate!.Value.Year, o.DeliveryDate!.Value.Month })
                        .Select(g => new
                        {
                            g.Key.Year,
                            g.Key.Month,
                            Amount = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Year)
                        .ThenBy(r => r.Month)
                        .ToListAsync();

                    return monthlyResults.Select(r => new RevenueDTO
                    {
                        Date = new DateTime(r.Year, r.Month, 1),
                        DisplayDate = $"{r.Year}-{r.Month:D2}",
                        Amount = r.Amount,
                        OrderCount = r.OrderCount
                    }).ToList();
                case "year":
                default:
                    // For yearly grouping - using a client-side evaluation approach to avoid EF Core translation issues
                    var yearlyResults = await completedOrders
                        .GroupBy(o => o.DeliveryDate!.Value.Year)
                        .Select(g => new
                        {
                            Year = g.Key,
                            Amount = g.Sum(o => o.TotalAmount),
                            OrderCount = g.Count()
                        })
                        .OrderBy(r => r.Year)
                        .ToListAsync();

                    return yearlyResults.Select(r => new RevenueDTO
                    {
                        Date = new DateTime(r.Year, 1, 1),
                        DisplayDate = r.Year.ToString(),
                        Amount = r.Amount,
                        OrderCount = r.OrderCount
                    }).ToList();
            }
        }
    }
}
