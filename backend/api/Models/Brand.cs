using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    // 1 - Many relationship with Category
    [Table("brands")]
    [Index(nameof(Name), nameof(Logo), IsUnique = true)]
    public class Brand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Brand name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Brand name can only contain letters and spaces")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(255)")]
        [StringLength(255, ErrorMessage = "Logo cannot exceed 255 characters")]
        [RegularExpression(@"^.+\.(png|jpg|jpeg)$", ErrorMessage = "Logo must be in .png, .jpg, .jpeg format")]
        public string Logo { get; set; } = string.Empty;

        [InverseProperty("Brand")]
        public virtual ICollection<Category> Categories { get; set; } = new HashSet<Category>();
    }
}