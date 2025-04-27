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

        [Column(TypeName = "timestamp")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime EndDate { get; set; }

        public ICollection<ProductDiscount> Products { get; set; } = new HashSet<ProductDiscount>();
        public bool IsActive => StartDate <= DateTime.Now && EndDate >= DateTime.Now;
    }
}