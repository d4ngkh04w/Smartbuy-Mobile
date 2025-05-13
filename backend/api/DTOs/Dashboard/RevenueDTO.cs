namespace api.DTOs.Dashboard
{
    public class RevenueDTO
    {
        public DateTime Date { get; set; }
        public string DisplayDate { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int OrderCount { get; set; }
    }
}
