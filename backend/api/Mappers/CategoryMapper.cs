using api.DTOs.Category;
using api.Models;

namespace api.Mappers
{
    public static class CategoryMapper
    {
        public static Category ToModel(this CreateCategoryDTO createCategoryDTO)
        {
            return new Category
            {
                Name = createCategoryDTO.Name,
                BrandId = createCategoryDTO.BrandId,
                Products = new HashSet<Product>()
            };
        }

        public static CategoryDTO ToDTO(this Category category)
        {
            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                BrandId = category.BrandId,
                BrandName = category.Brand?.Name ?? string.Empty,
                Products = category.Products.Select(p => p.ToDTO()).ToHashSet()
            };
        }
    }
}