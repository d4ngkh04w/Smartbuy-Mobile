using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDBContext _context;

        public ProductRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.ProductLine)
                .Include(p => p.Colors)
                .Include(p => p.Images)
                .Include(p => p.Discounts)
                .Include(p => p.Detail)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.ProductLine)
                .Include(p => p.Colors)
                .Include(p => p.Images)
                .Include(p => p.Discounts)
                .Include(p => p.Detail)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.IsActive = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Products.AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> ExistsByIdAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }
        public async Task<(List<Product> Items, int TotalItems)> GetPagedProductsAsync(int page, int pageSize)
        {
            var totalItems = await _context.Products.CountAsync(p => p.IsActive);
            
            var items = await _context.Products
                .Include(p => p.Images)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalItems);
        }

    }
}