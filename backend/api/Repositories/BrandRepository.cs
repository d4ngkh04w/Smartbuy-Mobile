using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using api.Queries;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

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
            // string sql = "INSERT INTO Brands (Name, Description, IsActive) VALUES (@Name, @Description, @IsActive); SELECT CAST(SCOPE_IDENTITY() as int)";
            // var id = await _db.Database.ExecuteSqlRawAsync(sql, 
            //     new MySqlParameter("@Name", brand.Name),
            //     new MySqlParameter("@Description", brand.Description ?? (object)DBNull.Value),
            //     new MySqlParameter("@IsActive", brand.IsActive));
            // brand.Id = id;
            // _db.Brands.Add(brand);
            // await _db.SaveChangesAsync();
            // return brand;
        }

        public async Task DeleteBrandAsync(Brand brand)
        {
            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Brand>> GetBrandsAsync(BrandQuery query)
        {
            var brandsQuery = _db.Brands.AsQueryable();

            // Only apply IsActive filter if it has a value
            if (query.IsActive.HasValue)
            {
                brandsQuery = brandsQuery.Where(b => b.IsActive == query.IsActive.Value);
            }
            // No else clause needed - when IsActive is null, we want all brands

            if (query.IncludeProductLines)
            {
                brandsQuery = brandsQuery.Include(b => b.ProductLines);
            }

            if (query.IncludeProducts)
            {
                brandsQuery = brandsQuery
                            .Include(b => b.ProductLines)
                                .ThenInclude(pl => pl.Products)
                                    .ThenInclude(p => p.Colors)
                                        .ThenInclude(c => c.Images)
                            .Include(b => b.ProductLines)
                                    .ThenInclude(pl => pl.Products)
                                        .ThenInclude(p => p.Detail)
                            .Include(b => b.ProductLines)
                                    .ThenInclude(pl => pl.Products)
                                        .ThenInclude(p => p.Discounts)
                                            .ThenInclude(d => d.Discount);
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

            return await brandsQuery.ToListAsync();
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
                brandQuery = brandQuery
                            .Include(b => b.ProductLines)
                                .ThenInclude(pl => pl.Products)
                                    .ThenInclude(p => p.Colors)
                                        .ThenInclude(c => c.Images)
                            .Include(b => b.ProductLines)
                                    .ThenInclude(pl => pl.Products)
                                        .ThenInclude(p => p.Detail)
                            .Include(b => b.ProductLines)
                                    .ThenInclude(pl => pl.Products)
                                        .ThenInclude(p => p.Discounts)
                                            .ThenInclude(d => d.Discount);
            }

            var baseQuery = brandQuery.Where(b => b.Id == id);

            if (query.IsActive.HasValue)
            {
                baseQuery = baseQuery.Where(b => b.IsActive == query.IsActive.Value);
            }

            return await baseQuery.FirstOrDefaultAsync();
        }

        public async Task<Brand> UpdateBrandAsync(Brand brand)
        {
            _db.Brands.Update(brand);
            await _db.SaveChangesAsync();
            return brand;
        }
    }
}