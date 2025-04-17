using api.Models;
using api.Queries;

namespace api.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetCategoryByIdAsync(int id, CategoryQuery? query = null);
        Task<IEnumerable<Category>> GetCategoriesAsync(CategoryQuery query);
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(Category category);
        Task<bool> CategoryExistAsync(string name);
    }
}