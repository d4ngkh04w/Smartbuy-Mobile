using api.Database;
using api.Interfaces.Repositories;
using api.Models;
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
                throw new Exception("Brand does not exist");
            db.Categories.Add(category);
            await db.SaveChangesAsync();
            return (await db.Categories
                .Include(c => c.Brand)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == category.Id))!;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category == null)
                return false;

            db.Categories.Remove(category);
            await db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CategoryExistAsync(string name)
        {
            return await db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await db.Categories
                .Include(c => c.Brand)
                .Include(c => c.Products)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await db.Categories
                .Include(c => c.Brand)
                .Include(c => c.Products)
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