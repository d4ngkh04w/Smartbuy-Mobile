using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("colors")]
    public class ProductColor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; } = string.Empty;

        public int ProductId { get; set; }

        [InverseProperty("Colors")]
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; } = null;
    }
}