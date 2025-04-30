using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class CarouselRepository : ICarouselRepository
    {
        private readonly AppDBContext _context;

        public CarouselRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CarouselImage>> GetAllAsync()
        {
            return await _context.CarouselImages
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        // Lấy hình ảnh carousel theo ID
        public async Task<CarouselImage?> GetByIdAsync(int id)
        {
            return await _context.CarouselImages
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<CarouselImage> CreateAsync(CarouselImage carouselImage)
        {
            await _context.CarouselImages.AddAsync(carouselImage);
            await _context.SaveChangesAsync();
            return carouselImage;
        }

        public async Task<bool> UpdateAsync(CarouselImage carouselImage)
        {
            _context.CarouselImages.Update(carouselImage);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var carouselImage = await _context.CarouselImages.FindAsync(id);
            if (carouselImage == null)
                return false;

            carouselImage.IsActive = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _context.CarouselImages.AnyAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<IEnumerable<CarouselImage>> GetActiveCarouselImagesAsync()
        {
            return await _context.CarouselImages
                .Where(c => c.IsActive)
                .ToListAsync();
        }
    }
}
