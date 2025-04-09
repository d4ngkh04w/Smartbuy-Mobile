using api.Models;

namespace api.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetCategoryByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<Category> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryExistAsync(string name);
    }
}