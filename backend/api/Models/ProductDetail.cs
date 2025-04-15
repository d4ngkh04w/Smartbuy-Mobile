using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace api.Models
{
    [Table("product_details")]
    public class ProductDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string Warranty { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string RAM { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string Storage { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Processor { get; set; } = string.Empty;

        [Column(TypeName = "varchar(10)")]
        public string ScreenSize { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string ScreenResolution { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string Battery { get; set; } = string.Empty;
        public int SimSlots { get; set; } = 0;

        [InverseProperty("Detail")]
        public Product? Product { get; set; } = null;
    }
}