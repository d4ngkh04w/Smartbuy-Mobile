using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using api.Queries;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class ProductLineRepository : IProductLineRepository
    {
        private readonly AppDBContext _db;

        public ProductLineRepository(AppDBContext db)
        {
            _db = db;
        }

        public async Task<ProductLine> CreateProductLineAsync(ProductLine productLine)
        {
            bool existsBrand = await _db.Brands.AnyAsync(b => b.Id == productLine.BrandId);
            if (!existsBrand)
                throw new Exception("Brand not found");

            _db.ProductLines.Add(productLine);
            await _db.SaveChangesAsync();

            return (await _db.ProductLines
                .Include(pl => pl.Brand)
                .Include(pl => pl.Products)
                .FirstOrDefaultAsync(pl => pl.Id == productLine.Id))!;
        }

        public async Task DeleteProductLineAsync(ProductLine productLine)
        {
            productLine.IsActive = false;
            _db.ProductLines.Update(productLine);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> ProductLineExistAsync(string name)
        {
            return await _db.ProductLines.AnyAsync(pl => pl.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<ProductLine>> GetProductLinesAsync(ProductLineQuery query)
        {
            var productLinesQuery = _db.ProductLines.AsQueryable();

            // Lọc theo trạng thái IsActive nếu có
            if (query.IsActive.HasValue)
            {
                productLinesQuery = productLinesQuery.Where(pl => pl.IsActive == query.IsActive.Value);
            }

            // Filter by brand ID if specified
            if (query.BrandId.HasValue)
            {
                productLinesQuery = productLinesQuery.Where(pl => pl.BrandId == query.BrandId.Value);
            }

            if (query.IncludeProducts)
            {
                productLinesQuery = productLinesQuery.Include(pl => pl.Products);
            }

            productLinesQuery = query.SortBy.ToLower() switch
            {
                "id" => productLinesQuery.OrderBy(pl => pl.Id),
                "name" => productLinesQuery.OrderBy(pl => pl.Name),
                _ => productLinesQuery.OrderBy(pl => pl.Name),
            };

            if (query.IsDescending)
            {
                productLinesQuery = productLinesQuery.Reverse();
            }

            // Thay vì chỉ Include Brand, cần giữ nguyên việc Include Products nếu đã được yêu cầu
            var query_with_brand = productLinesQuery.Include(pl => pl.Brand);
            
            return await query_with_brand.ToListAsync();
        }

        public async Task<ProductLine?> GetProductLineByIdAsync(int id, ProductLineQuery? query = null)
        {
            if (query == null)
            {
                return await _db.ProductLines.FindAsync(id);
            }

            var productLineQuery = _db.ProductLines.AsQueryable();

            if (query.IncludeProducts)
            {
                productLineQuery = productLineQuery.Include(pl => pl.Products);
            }

            return await productLineQuery
                .Include(pl => pl.Brand)
                .FirstOrDefaultAsync(pl => pl.Id == id);
        }

        public async Task<bool> UpdateProductLineAsync(ProductLine productLine)
        {
            _db.Entry(productLine).State = EntityState.Modified;
            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.ProductLines.AnyAsync(pl => pl.Id == productLine.Id))
                    return false;
                else
                    throw;
            }
        }
    }
}