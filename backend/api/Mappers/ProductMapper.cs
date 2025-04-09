using api.DTOs.Product;
using api.Models;

namespace api.Mappers
{
    public static class ProductMapper
    {
        public static Product ToModel(this ProductDTO productDTO)
        {
            return new Product
            {
                Id = productDTO.Id,
                CategoryId = productDTO.CategoryId,
            };
        }

        public static ProductDTO ToDTO(this Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
            };
        }

    }
}