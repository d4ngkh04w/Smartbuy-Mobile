using System;
using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Carousel
{
    public class CarouselDTO
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public string LinkUrl { get; set; } = "#";

        [Required(ErrorMessage = "Status is required")]
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}