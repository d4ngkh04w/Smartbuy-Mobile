namespace api.DTOs.Product
{
    public class ProductPagiDTO
    {
        public int TotalItems { get; set; }
        public List<ProductSummaryDTO>? Items { get; set; }
    }
}