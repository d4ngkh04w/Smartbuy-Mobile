using api.Database;
using api.DTOs.Category;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Queries;

namespace api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository repo;
        public CategoryService(ICategoryRepository repo)
        {
            this.repo = repo;
        }

        public async Task<(bool Success, string? ErrorMessage, CategoryDTO? Category)> CreateCategoryAsync(CreateCategoryDTO categoryDTO)
        {
            try
            {
                if (await repo.CategoryExistAsync(categoryDTO.Name))
                    return (false, "Category already exists", null);

                var createdCategory = await repo.CreateCategoryAsync(categoryDTO.ToModel());
                return (true, null, createdCategory.ToDTO());
            }
            catch (Exception)
            {
                return (false, "Error creating category", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await repo.GetCategoryByIdAsync(id);
                if (category == null)
                    return (false, "Category not found");

                await repo.DeleteCategoryAsync(category);

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "Error deleting category");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<CategoryDTO>? Categories)> GetCategoriesAsync(CategoryQuery query)
        {
            try
            {
                var categories = await repo.GetCategoriesAsync(query);
                if (categories == null || !categories.Any())
                    return (false, "Not found categories", null);

                var categoryDTOs = categories.Select(c => c.ToDTO()).ToList();
                return (true, null, categoryDTOs);
            }
            catch (Exception)
            {
                return (false, "Error retrieving categories", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CategoryDTO? Category)> GetCategoryByIdAsync(int id, CategoryQuery query)
        {
            try
            {
                var category = await repo.GetCategoryByIdAsync(id, query);
                if (category == null)
                    return (false, "Category not found", null);

                return (true, null, category.ToDTO());
            }
            catch (Exception)
            {
                return (false, "Error retrieving category", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(int id, UpdateCategoryDTO categoryDTO)
        {
            try
            {
                if (id <= 0)
                    return (false, "Invalid category ID");

                var category = await repo.GetCategoryByIdAsync(id);
                if (category == null)
                    return (false, "Category not found");

                if (categoryDTO.Name != null && categoryDTO.Name != category.Name)
                    category.Name = categoryDTO.Name;
                if (categoryDTO.BrandId != null && categoryDTO.BrandId != category.BrandId)
                    category.BrandId = (int)categoryDTO.BrandId;

                var success = await repo.UpdateCategoryAsync(category);
                if (!success)
                    return (false, "Error updating category");

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "Error updating category");
            }
        }
    }
}