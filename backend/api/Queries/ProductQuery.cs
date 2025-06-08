namespace api.Queries
{
    public class ProductQuery
    {
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10;
        public string? Search { get; set; } = null;
        public string? SortBy { get; set; } = "newest";
        public string? BrandName { get; set; } = null;
        public int? ProductLineId { get; set; } = null;
        public decimal? MinPrice { get; set; } = null;
        public decimal? MaxPrice { get; set; } = null;
        public bool? IsActive { get; set; } = true;
    }
}