namespace api.DTOs.Comment
{
    public class ProductRatingStatsDTO
    {
        public double AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public Dictionary<int, RatingDetailDTO> RatingDistribution { get; set; } = new Dictionary<int, RatingDetailDTO>();
    }

    public class RatingDetailDTO
    {
        public int Count { get; set; }
        public double Percentage { get; set; }
    }
}
