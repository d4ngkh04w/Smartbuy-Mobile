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

        public async Task<IEnumerable<Brand>> GetAllBrandsAsync()
        {
            return await db.Brands.Include(b => b.Categories).ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            return await db.Brands.Include(b => b.Categories).FirstOrDefaultAsync(b => b.Id == id);
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