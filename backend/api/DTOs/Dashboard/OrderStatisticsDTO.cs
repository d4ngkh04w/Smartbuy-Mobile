namespace api.DTOs.Dashboard
{
    public class OrderStatisticsDTO
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal OrderChange { get; set; }
        public decimal RevenueChange { get; set; }
        public decimal AvgChange { get; set; }
        public decimal CompletionChange { get; set; }
    }

    public class OrderStatusDistributionDTO
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class PaymentMethodStatDTO
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }    public class OrderReportDTO
    {
        public OrderStatisticsDTO Statistics { get; set; } = new();
        public List<OrderStatusDistributionDTO> StatusDistribution { get; set; } = new();
        public List<PaymentMethodStatDTO> PaymentMethods { get; set; } = new();
        public List<RevenueChartDTO> RevenueChartData { get; set; } = new();
    }
}
