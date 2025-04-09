using api.Database;
using api.DTOs.Category;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;

namespace api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository repo;
        private readonly ILogger<BrandService> logger;
        public CategoryService(ICategoryRepository repo, ILogger<BrandService> logger)
        {
            this.repo = repo;
            this.logger = logger;
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating category");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int id)
        {
            try
            {
                var success = await repo.DeleteCategoryAsync(id);
                if (!success)
                    return (false, "Category not found");

                return (true, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting category");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<CategoryDTO>? Categories)> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await repo.GetAllCategoriesAsync();
                if (categories == null || !categories.Any())
                    return (false, "Not Found", null);

                var categoryDTOs = categories.Select(c => c.ToDTO()).ToList();
                return (true, null, categoryDTOs);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving categories");
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, CategoryDTO? Category)> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await repo.GetCategoryByIdAsync(id);
                if (category == null)
                    return (false, "Not Found", null);

                var categoryDTO = category.ToDTO();
                return (true, null, categoryDTO);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving category");
                return (false, ex.Message, null);
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating category");
                return (false, ex.Message);
            }
        }
    }
}