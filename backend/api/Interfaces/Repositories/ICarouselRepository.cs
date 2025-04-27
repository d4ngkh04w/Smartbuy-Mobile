using api.Models;

namespace api.Interfaces.Repositories
{
    public interface ICarouselRepository
    {
        Task<IEnumerable<CarouselImage>> GetAllAsync();

        Task<CarouselImage?> GetByIdAsync(int id);

        Task<CarouselImage> CreateAsync(CarouselImage carouselImage);

        Task<bool> UpdateAsync(CarouselImage carouselImage);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsByIdAsync(int id);

        Task<IEnumerable<CarouselImage>> GetActiveCarouselImagesAsync();
    }
}
