using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDBContext _db;

        public ProductRepository(AppDBContext context)
        {
            _db = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.Products
                .Include(p => p.ProductLine)
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Include(p => p.Discounts)
                .Include(p => p.Detail)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.Products
                .Include(p => p.ProductLine)
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Include(p => p.Discounts)
                .Include(p => p.Detail)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return product;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(Product product)
        {
            if (product == null)
                return false;

            product.ManuallyDeactivated = true;
            product.UpdatedAt = DateTime.Now;

            _db.Products.Update(product);
            return await _db.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _db.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<(List<Product> Items, int TotalItems)> GetPagedProductsAsync(int page, int pageSize)
        {
            var totalItems = await _db.Products.CountAsync(p => p.IsActive);

            var items = await _db.Products
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<List<Product>> GetProductsByProductLineIdAsync(int productLineId)
        {
            return await _db.Products
                .Where(p => p.ProductLineId == productLineId && p.IsActive)
                .ToListAsync();
        }

        public async Task<ProductColor> AddColorAsync(ProductColor color)
        {
            await _db.Colors.AddAsync(color);
            await _db.SaveChangesAsync();
            return color;
        }

        public async Task<ProductImage> AddImageAsync(ProductImage image)
        {
            await _db.ProductImages.AddAsync(image);
            await _db.SaveChangesAsync();
            return image;
        }
    }
}