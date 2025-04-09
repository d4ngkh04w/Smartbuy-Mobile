using api.Database;
using api.Interfaces.Services;
using api.Models;
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

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            db.Brands.Add(brand);
            await db.SaveChangesAsync();
            return brand;
        }

        public async Task<bool> DeleteBrandAsync(int? id = null, string? name = null)
        {
            var brand = await GetBrandAsync(id, name);
            if (brand == null)
                return false;

            db.Brands.Remove(brand);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await db.Brands.Include(b => b.Categories).ToListAsync();
        }

        public async Task<Brand?> GetBrandAsync(int? id = null, string? name = null)
        {
            var query = db.Brands.AsQueryable();
            if (id.HasValue)
                query = query.Where(b => b.Id == id.Value);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(b => b.Name == name);

            return await query.Include(b => b.Categories).FirstOrDefaultAsync();
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