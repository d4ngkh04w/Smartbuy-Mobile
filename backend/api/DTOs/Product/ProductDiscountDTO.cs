namespace api.DTOs.Product
{
    public class ProductDiscountDTO
    {
        public int Id { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}