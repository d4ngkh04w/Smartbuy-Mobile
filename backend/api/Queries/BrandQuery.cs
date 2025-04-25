namespace api.Queries
{
    public class BrandQuery
    {
        public bool IncludeProductLines { get; set; } = false;
        public bool IncludeProducts { get; set; } = false;
        public string SortBy { get; set; } = "Name";
        public bool IsDescending { get; set; } = false;
        public bool? IsActive { get; set; } = null;
    }
}