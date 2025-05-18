namespace api.DTOs.Discount
{
    public class DiscountDTO
    {
        public int Id { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty; // (Active, Expired, Upcoming)
    }
}
