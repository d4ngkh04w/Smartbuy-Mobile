using api.DTOs.Carousel;

namespace api.Interfaces.Services
{
    public interface ICarouselService
    {
        Task<(bool Success, string? ErrorMessage, CarouselDTO? Carousel)> CreateAsync(IFormFile imageFile, CarouselDTO dto);
        Task<(bool Success, string? ErrorMessage, CarouselDTO? Carousel)> UpdateAsync(int id, IFormFile? imageFile, CarouselDTO dto);
        Task<(bool Success, string? ErrorMessage, CarouselDTO? Carousel)> GetByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage, IEnumerable<CarouselDTO>? Carousels)> GetAllAsync();
        Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
        Task<(bool Success, string? ErrorMessage, IEnumerable<CarouselDTO>? Carousels)> GetActiveAsync();
    }
}