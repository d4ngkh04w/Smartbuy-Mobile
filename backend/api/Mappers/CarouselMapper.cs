using api.DTOs.Carousel;
using api.Models;
using System;

namespace api.Mappers
{
    public static class CarouselMapper
    {
        public static CarouselDTO ToCarouselDTO(this CarouselImage entity)
        {
            return new CarouselDTO
            {
                Id = entity.Id,
                Title = entity.Title,
                ImagePath = entity.ImagePath,
                LinkUrl = entity.LinkUrl,
                IsActive = entity.IsActive,
                CreatedAt = entity.CreatedAt
            };
        }

        public static CarouselImage ToModel(this CarouselDTO dto, string savedImagePath)
        {
            return new CarouselImage
            {
                Id = dto.Id ?? 0,
                Title = dto.Title,
                ImagePath = savedImagePath,
                LinkUrl = dto.LinkUrl,
                IsActive = dto.IsActive,
                CreatedAt = dto.Id == null ? DateTime.Now : dto.CreatedAt
            };
        }

        public static void UpdateFromDTO(this CarouselImage entity, CarouselDTO dto, string? savedImagePath = null)
        {
            entity.Title = dto.Title;
            entity.LinkUrl = dto.LinkUrl;
            entity.IsActive = dto.IsActive;

            if (!string.IsNullOrEmpty(savedImagePath))
            {
                entity.ImagePath = savedImagePath;
            }
        }
    }
}