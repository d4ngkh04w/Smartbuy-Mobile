using api.Database;
using api.Interfaces.Services;
using api.Models;
using api.Queries;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly AppDBContext db;
        public BrandRepository(AppDBContext db)
        {
            this.db = db;
        }

        public async Task<bool> BrandExistsAsync(string name)
        {
            return await db.Brands.AnyAsync(b => b.Name == name);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            return brand;
        }

        public async Task DeleteBrandAsync(Brand brand)
        {
            db.Brands.Remove(brand);
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Brand>> GetBrandsAsync(BrandQuery query)
        {
            var brandsQuery = db.Brands.AsQueryable();

            if (query.IsActive)
            {
                brandsQuery = brandsQuery.Where(b => b.IsActive == query.IsActive);
            }

            if (query.IncludeCategories)
            {
                brandsQuery = brandsQuery.Include(b => b.Categories);
            }

            if (query.IncludeProducts)
            {
                brandsQuery = brandsQuery.Include(b => b.Categories).ThenInclude(c => c.Products);
            }

            brandsQuery = query.SortBy.ToLower() switch
            {
                "id" => brandsQuery.OrderBy(b => b.Id),
                "name" => brandsQuery.OrderBy(b => b.Name),
                _ => brandsQuery.OrderBy(b => b.Name),
            };

            if (query.IsDescending)
            {
                brandsQuery = brandsQuery.OrderByDescending(b => b.Id);
            }

            return await brandsQuery.ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id, BrandQuery? query = null)
        {
            if (query == null)
            {
                return await db.Brands.FindAsync(id);
            }

            var brandQuery = db.Brands.AsQueryable();

            if (query.IncludeCategories)
            {
                brandQuery = brandQuery.Include(b => b.Categories);
            }

            if (query.IncludeProducts)
            {
                brandQuery = brandQuery.Include(b => b.Categories).ThenInclude(c => c.Products);
            }

            return await brandQuery.FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> UpdateBrandAsync(Brand brand)
        {
            db.Entry(brand).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await db.Brands.AnyAsync(b => b.Id == brand.Id))
                    return false;
                else
                    throw;
            }
        }
    }
}