using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    [Table("carousel_images")]
    [Index(nameof(Title), IsUnique = true)]
    public class CarouselImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "varchar(255)")]
        public string ImagePath { get; set; } = string.Empty;

        [Column(TypeName = "varchar(100)")]
        public string Title { get; set; } = string.Empty;

        [Column(TypeName = "varchar(255)")]
        public string LinkUrl { get; set; } = string.Empty;

        [Column(TypeName = "bit")]
        public bool IsActive { get; set; } = true;

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
