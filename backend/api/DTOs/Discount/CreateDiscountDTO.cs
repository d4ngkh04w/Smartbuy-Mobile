using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Discount
{
    public class CreateDiscountDTO
    {
        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        public decimal DiscountPercentage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount cannot be negative")]
        public decimal DiscountAmount { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }
}