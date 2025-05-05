namespace api.Queries
{
    public class ProductLineQuery
    {
        public bool IncludeProducts { get; set; } = false;
        public string SortBy { get; set; } = "Name";
        public bool IsDescending { get; set; } = false;
        public bool? IsActive { get; set; } = null; // Thêm thuộc tính này để hỗ trợ lọc theo trạng thái
        public int? BrandId { get; set; } = null; // Thêm thuộc tính này để hỗ trợ lọc theo brand
    }
}