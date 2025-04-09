using api.DTOs.Category;
using api.Models;

namespace api.Interfaces.Services
{
    public interface ICategoryService
    {
        Task<(bool Success, string? ErrorMessage, IEnumerable<CategoryDTO>? Categories)> GetAllCategoriesAsync();
        Task<(bool Success, string? ErrorMessage, CategoryDTO? Category)> GetCategoryByIdAsync(int id);
        Task<(bool Success, string? ErrorMessage, CategoryDTO? Category)> CreateCategoryAsync(CreateCategoryDTO categoryDTO);
        Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id);
        Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(int id, UpdateCategoryDTO categoryDTO);
    }
}