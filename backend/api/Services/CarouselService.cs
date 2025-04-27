using api.DTOs.Carousel;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;
using api.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Services
{
    public class CarouselService : ICarouselService
    {
        private readonly ICarouselRepository _carouselRepo;
        private readonly IWebHostEnvironment _env;

        public CarouselService(ICarouselRepository carouselRepo, IWebHostEnvironment env)
        {
            _carouselRepo = carouselRepo;
            _env = env;
        }

        public async Task<(bool Success, string? ErrorMessage, CarouselDTO? Carousel)> CreateAsync(IFormFile imageFile, CarouselDTO dto)
        {
            try
            {
                var saveRes = await ImageHelper.SaveImageAsync(imageFile, _env.WebRootPath, "carousels", 5 * 1024 * 1024);
                if (!saveRes.Success) return (false, saveRes.ErrorMessage, null);

                var entity = new CarouselImage
                {
                    Title = dto.Title,
                    ImagePath = saveRes.FilePath!,
                    LinkUrl = dto.LinkUrl,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.Now
                };

                var created = await _carouselRepo.CreateAsync(entity);
                return (true, null, created.ToCarouselDTO());
            }
            catch (Exception ex)
            {
                return (false, $"Error creating carousel: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CarouselDTO? Carousel)> UpdateAsync(int id, IFormFile? imageFile, CarouselDTO dto)
        {
            try
            {
                var existing = await _carouselRepo.GetByIdAsync(id);
                if (existing == null) return (false, "Carousel not found", null);

                string? newImagePath = null;
                if (imageFile != null)
                {
                    var saveRes = await ImageHelper.SaveImageAsync(imageFile, _env.WebRootPath, "carousels", 5 * 1024 * 1024);
                    if (!saveRes.Success) return (false, saveRes.ErrorMessage, null);
                    newImagePath = saveRes.FilePath;

                    // Delete old image
                    if (!string.IsNullOrEmpty(existing.ImagePath))
                    {
                        var fullPath = Path.Combine(_env.WebRootPath, existing.ImagePath);
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                        }
                    }
                }

                existing.Title = dto.Title;
                existing.LinkUrl = dto.LinkUrl;
                existing.IsActive = dto.IsActive;
                if (newImagePath != null) existing.ImagePath = newImagePath;

                var updated = await _carouselRepo.UpdateAsync(existing);
                return updated ? (true, null, existing.ToCarouselDTO()) : (false, "Update failed", null);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating carousel: {ex.Message}", null);
            }
        }


        public async Task<(bool Success, string? ErrorMessage, CarouselDTO? Carousel)> GetByIdAsync(int id)
        {
            try
            {
                var entity = await _carouselRepo.GetByIdAsync(id);
                if (entity == null) return (false, "Carousel not found", null);

                return (true, null, entity.ToCarouselDTO());
            }
            catch (Exception ex)
            {
                return (false, $"Error fetching carousel: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<CarouselDTO>? Carousels)> GetAllAsync()
        {
            try
            {
                var entities = await _carouselRepo.GetAllAsync();
                if (entities == null || !entities.Any())
                    return (false, "No carousels found", null);

                return (true, null, entities.Select(e => e.ToCarouselDTO()));
            }
            catch (Exception ex)
            {
                return (false, $"Error fetching carousels: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
        {
            try
            {
                var existing = await _carouselRepo.GetByIdAsync(id);
                if (existing == null) return (false, "Carousel not found");

                // Delete associated image file
                if (!string.IsNullOrEmpty(existing.ImagePath))
                {
                    ImageHelper.DeleteImage(Path.Combine(_env.WebRootPath, existing.ImagePath));
                }
                var result = await _carouselRepo.DeleteAsync(id);
                return result ? (true, null) : (false, "Delete failed");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting carousel: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<CarouselDTO>? Carousels)> GetActiveAsync()
        {
            try
            {
                var entities = await _carouselRepo.GetActiveCarouselImagesAsync();
                if (entities == null || !entities.Any())
                    return (false, "No active carousels found", null);

                return (true, null, entities.Select(e => e.ToCarouselDTO()));
            }
            catch (Exception ex)
            {
                return (false, $"Error fetching active carousels: {ex.Message}", null);
            }
        }
    }
}