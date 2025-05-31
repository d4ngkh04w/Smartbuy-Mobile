using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("discounts")]
    public class Discount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Column(TypeName = "decimal(5, 2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "timestamp")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime EndDate { get; set; }

        public ICollection<ProductDiscount> Products { get; set; } = new HashSet<ProductDiscount>();

        public decimal CalculateDiscountedPrice(decimal originalPrice)
        {
            decimal discountedPrice = originalPrice;

            if (DateTime.Now < StartDate || DateTime.Now > EndDate)
            {
                return originalPrice;
            }

            if (DiscountPercentage > 0)
            {
                discountedPrice -= originalPrice * (DiscountPercentage / 100);
            }
            else if (DiscountAmount > 0)
            {
                discountedPrice -= DiscountAmount;
            }

            return Math.Max(discountedPrice, 0);
        }

        public override string ToString()
        {
            if (DateTime.Now < StartDate || DateTime.Now > EndDate)
            {
                return string.Empty;
            }

            if (DiscountPercentage > 0)
            {
                return $"-{DiscountPercentage}%";
            }
            else if (DiscountAmount > 0)
            {
                return $"-{DiscountAmount.ToString("N0", new System.Globalization.CultureInfo("vi-VN"))}₫";
            }

            return string.Empty;
        }
    }
}