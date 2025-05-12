namespace api.Queries
{
    public class ProductLineQuery
    {
        public bool IncludeProducts { get; set; } = false;
        public string SortBy { get; set; } = "Name";
        public bool IsDescending { get; set; } = false;
        public bool? IsActive { get; set; } = null;
        public int? BrandId { get; set; } = null;
    }
}