using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    // Many - 1 relationship with Brand
    [Table("categories")]
    [Index(nameof(Name), nameof(BrandId), IsUnique = true)]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int BrandId { get; set; }

        [ForeignKey(nameof(BrandId))]
        [InverseProperty("Categories")]
        public Brand? Brand { get; set; } = null!;

        [InverseProperty("Category")]
        public virtual ICollection<Product>? Products { get; set; } = null!;
    }
}