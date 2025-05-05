using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("product_details")]
    public class ProductDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? Id { get; set; }

        public int? WarrantyMonths { get; set; }

        public int? RAMInGB { get; set; }

        public int? StorageInGB { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? Processor { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string? OperatingSystem { get; set; } = string.Empty;

        [Column(TypeName = "decimal(3, 1)")]
        public decimal? ScreenSizeInch { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string? ScreenResolution { get; set; } = string.Empty;

        public int? BatteryCapacityMAh { get; set; }

        public int? SimSlots { get; set; } = 1;

        public int? ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; } = null;
    }
}