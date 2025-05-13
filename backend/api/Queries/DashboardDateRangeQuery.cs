using System.ComponentModel.DataAnnotations;

namespace api.Queries
{
    public class DashboardDateRangeQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [RegularExpression("^(day|month|year)$", ErrorMessage = "Period must be one of: day, month, year")]
        public string? Period { get; set; }

        public string? SortBy { get; set; } = "quantity"; // quantity or revenue
    }
}
