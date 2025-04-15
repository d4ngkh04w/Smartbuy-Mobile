using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using api.Queries;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDBContext db;
        public CategoryRepository(AppDBContext db)
        {
            this.db = db;
        }
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            bool existsBrand = await db.Brands.AnyAsync(b => b.Id == category.BrandId);
            if (!existsBrand)
                throw new Exception("Brand not found");
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return (await db.Categories
                .Include(c => c.Brand)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == category.Id))!;
        }

        public async Task DeleteCategoryAsync(Category category)
        {
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
        }

        public async Task<bool> CategoryExistAsync(string name)
        {
            return await db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync(CategoryQuery query)
        {
            var categoriesQuery = db.Categories.AsQueryable();

            if (query.IncludeProducts)
            {
                categoriesQuery = categoriesQuery.Include(c => c.Products);
            }

            categoriesQuery = query.SortBy.ToLower() switch
            {
                "id" => categoriesQuery.OrderBy(c => c.Id),
                "name" => categoriesQuery.OrderBy(c => c.Name),
                _ => categoriesQuery.OrderBy(c => c.Name),
            };

            if (query.IsDescending)
            {
                categoriesQuery = categoriesQuery.Reverse();
            }

            return await categoriesQuery.Include(c => c.Brand).ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id, CategoryQuery? query = null)
        {
            if (query == null)
            {
                return await db.Categories.FindAsync(id);
            }
            var categoryQuery = db.Categories.AsQueryable();
            if (query.IncludeProducts)
            {
                categoryQuery = categoryQuery.Include(c => c.Products);
            }
            return await categoryQuery.Include(c => c.Brand)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            db.Entry(category).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await db.Categories.AnyAsync(c => c.Id == category.Id))
                    return false;
                else
                    throw;
            }
        }
    }
}