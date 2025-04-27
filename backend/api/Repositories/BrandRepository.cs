using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using api.Queries;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class BrandRepository : IBrandRepository
    {
        private readonly AppDBContext _db;

        public BrandRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<bool> BrandExistsAsync(string name)
        {
            return await _db.Brands.AnyAsync(b => b.Name == name);
        }

        public async Task<Brand> CreateBrandAsync(Brand brand)
        {
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
            return brand;
        }

        public async Task DeleteBrandAsync(Brand brand)
        {
            brand.IsActive = false;
            _db.Brands.Update(brand);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Brand>> GetBrandsAsync(BrandQuery query)
        {
            var brandsQuery = _db.Brands.AsQueryable();

            if (query.IsActive)
            {
                brandsQuery = brandsQuery.Where(b => b.IsActive == query.IsActive);
            }

            if (query.IncludeProductLines)
            {
                brandsQuery = brandsQuery.Include(b => b.ProductLines);
            }

            if (query.IncludeProducts)
            {
                brandsQuery = brandsQuery.Include(b => b.ProductLines)
                    .ThenInclude(pl => pl.Products)
                    .ThenInclude(p => p.Colors)
                    .Include(b => b.ProductLines)
                    .ThenInclude(pl => pl.Products)
                    .ThenInclude(p => p.Images)
                    .Include(b => b.ProductLines)
                    .ThenInclude(pl => pl.Products)
                    .ThenInclude(p => p.Detail)
                    .Include(b => b.ProductLines)
                    .ThenInclude(pl => pl.Products)
                    .ThenInclude(p => p.Discounts);
            }

            brandsQuery = query.SortBy.ToLower() switch
            {
                "id" => brandsQuery.OrderBy(b => b.Id),
                "name" => brandsQuery.OrderBy(b => b.Name),
                _ => brandsQuery.OrderBy(b => b.Name),
            };

            if (query.IsDescending)
            {
                brandsQuery = brandsQuery.Reverse();
            }

            return await brandsQuery.Where(b => b.IsActive == query.IsActive).ToListAsync();
        }

        public async Task<Brand?> GetBrandByIdAsync(int id, BrandQuery? query = null)
        {
            if (query == null)
            {
                return await _db.Brands.FindAsync(id);
            }

            var brandQuery = _db.Brands.AsQueryable();

            if (query.IncludeProductLines)
            {
                brandQuery = brandQuery.Include(b => b.ProductLines);
            }

            if (query.IncludeProducts)
            {
                brandQuery = brandQuery.Include(b => b.ProductLines).ThenInclude(pl => pl.Products);
            }

            return await brandQuery.FirstOrDefaultAsync(b => b.Id == id && b.IsActive == query.IsActive);
        }

        public async Task<bool> UpdateBrandAsync(Brand brand)
        {
            _db.Entry(brand).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Brands.AnyAsync(b => b.Id == brand.Id))
                    return false;
                else
                    throw;
            }
        }
    }
}