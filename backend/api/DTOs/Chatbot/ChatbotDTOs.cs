namespace api.DTOs.Chatbot
{
    public class ChatMessageDTO
    {
        public string Message { get; set; } = string.Empty;
        public string? SessionId { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    public class ChatResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsUser { get; set; } = false;
        public bool IsError { get; set; } = false;
        public List<string>? SuggestedActions { get; set; }
    }

    public class ProductContextDTO
    {
        public List<ProductSummaryDTO>? AvailableProducts { get; set; }
        public List<string>? Brands { get; set; }
        public List<string>? Categories { get; set; }
        public List<PromotionDTO>? CurrentPromotions { get; set; }
    }

    public class ProductSummaryDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class PromotionDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? DiscountPercent { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}
