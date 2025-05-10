using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using api.Queries;
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

        public async Task<Product> UpdateAsync(Product product)
        {
            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            return product;
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

        // public async Task<(List<Product> Items, int TotalItems)> GetPagedProductsAsync(
        //     int page,
        //     int pageSize,
        //     string? search = null,
        //     string? sortBy = "newest",
        //     string? brand = null,
        //     decimal? minPrice = null,
        //     decimal? maxPrice = null)
        // {
        //     var query = _db.Products
        //         .Include(p => p.Colors)
        //             .ThenInclude(c => c.Images)
        //         .Include(p => p.ProductLine)
        //         .Where(p => p.IsActive)
        //         .AsQueryable();

        //     if (!string.IsNullOrWhiteSpace(search))
        //     {
        //         string lowerKey = search.Trim().ToLower();
        //         query = query.Where(p =>
        //             p.Name.ToLower().Contains(lowerKey));
        //     }

        //     if (!string.IsNullOrWhiteSpace(brand))
        //     {
        //         string lowerBrand = brand.Trim().ToLower();
        //         var brandInDb = _db.Brands
        //             .Where(b => b.Name.ToLower().Contains(lowerBrand))
        //             .FirstOrDefault();

        //         if (brandInDb != null)
        //         {
        //             query = query.Where(p => p.ProductLine!.BrandId == brandInDb.Id);
        //         }

        //     }


        //     if (minPrice.HasValue)
        //     {
        //         query = query.Where(p => p.SalePrice >= minPrice.Value);
        //     }

        //     if (maxPrice.HasValue)
        //     {
        //         query = query.Where(p => p.SalePrice <= maxPrice.Value);
        //     }

        //     switch (sortBy)
        //     {
        //         case "oldest":
        //             query = query.OrderBy(p => p.CreatedAt);
        //             break;
        //         case "priceInc":
        //             query = query.OrderBy(p => p.SalePrice);
        //             break;
        //         case "priceDesc":
        //             query = query.OrderByDescending(p => p.SalePrice);
        //             break;
        //         default: // newest
        //             query = query.OrderByDescending(p => p.CreatedAt);
        //             break;
        //     }

        //     var totalItems = await query.CountAsync();

        //     var items = await query
        //         .Skip((page - 1) * pageSize)
        //         .Take(pageSize)
        //         .AsNoTracking()
        //         .ToListAsync();

        //     return (items, totalItems);
        // }

        public async Task<(List<Product> Items, int TotalItems)> GetPagedProductsAsync(ProductQuery productQuery)
        {
            var query = _db.Products
                .Include(p => p.Colors)
                    .ThenInclude(c => c.Images)
                .Include(p => p.ProductLine)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(productQuery.Search))
            {
                string lowerKey = productQuery.Search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(lowerKey));
            }

            if (!string.IsNullOrWhiteSpace(productQuery.BrandName))
            {
                string lowerBrand = productQuery.BrandName.Trim().ToLower();
                var brandInDb = _db.Brands
                    .Where(b => b.Name.ToLower().Contains(lowerBrand))
                    .FirstOrDefault();

                if (brandInDb != null)
                {
                    query = query.Where(p => p.ProductLine!.BrandId == brandInDb.Id);
                }
            }

            if (productQuery.MinPrice.HasValue)
            {
                query = query.Where(p => p.SalePrice >= productQuery.MinPrice.Value);
            }

            if (productQuery.MaxPrice.HasValue)
            {
                query = query.Where(p => p.SalePrice <= productQuery.MaxPrice.Value);
            }

            switch (productQuery.SortBy)
            {
                case "oldest":
                    query = query.OrderBy(p => p.CreatedAt);
                    break;
                case "priceInc":
                    query = query.OrderBy(p => p.SalePrice);
                    break;
                case "priceDesc":
                    query = query.OrderByDescending(p => p.SalePrice);
                    break;
                default: // newest
                    query = query.OrderByDescending(p => p.CreatedAt);
                    break;
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip(((productQuery.Page ?? 1) - 1) * (productQuery.PageSize ?? 10))
                .Take(productQuery.PageSize ?? 10)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalItems);
        }

        public async Task<List<Product>> GetProductsByProductLineIdAsync(int productLineId)
        {
            return await _db.Products
                .Where(p => p.ProductLineId == productLineId)
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