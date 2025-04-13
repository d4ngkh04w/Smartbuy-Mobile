namespace api.Queries
{
    public class CategoryQuery
    {
        public bool IncludeProducts { get; set; } = false;
        public string SortBy { get; set; } = "Name";
        public bool IsDescending { get; set; } = false;
    }
}