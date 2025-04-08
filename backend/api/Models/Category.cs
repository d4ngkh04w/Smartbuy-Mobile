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
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Categorie name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Categorie name can only contain letters and spaces")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int BrandId { get; set; }

        [ForeignKey(nameof(BrandId))]
        [InverseProperty("Categories")]
        public required Brand Brand { get; set; }
    }
}