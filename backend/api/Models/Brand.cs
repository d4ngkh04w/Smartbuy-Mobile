using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    // 1 - Many relationship with ProductLine
    [Table("brands")]
    [Index(nameof(Name), IsUnique = true)]
    public class Brand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(255)")]
        public string? Logo { get; set; }

        [Column(TypeName = "varchar(2000)")]
        public string? Description { get; set; }

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        public ICollection<ProductLine> ProductLines { get; set; } = new HashSet<ProductLine>();
    }
}