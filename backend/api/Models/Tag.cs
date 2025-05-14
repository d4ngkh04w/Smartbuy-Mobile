// using System.ComponentModel.DataAnnotations;
// using System.ComponentModel.DataAnnotations.Schema;
// using Microsoft.EntityFrameworkCore;

// namespace api.Models
// {
//     [Table("tags")]
//     [Index(nameof(Name), IsUnique = true)]
//     public class Tag
//     {
//         [Key]
//         [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//         public int Id { get; set; }

//         [Column(TypeName = "varchar(50)")]
//         public string Name { get; set; } = string.Empty;

//         public ICollection<ProductTag> ProductTags { get; set; } = new HashSet<ProductTag>();
//     }
// }