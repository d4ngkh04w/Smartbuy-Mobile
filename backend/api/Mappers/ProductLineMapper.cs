using api.DTOs.ProductLine;
using api.Models;
using api.Utils;

namespace api.Mappers
{
    public static class ProductLineMapper
    {
        public static ProductLine ToModel(this CreateProductLineDTO createProductLineDTO)
        {
            return new ProductLine
            {
                Name = createProductLineDTO.Name,
                Description = createProductLineDTO.Description,
                BrandId = createProductLineDTO.BrandId,
                IsActive = true,
                CreatedAt = DateTimeUtils.FormatDateTime(DateTime.Now),
                Products = new HashSet<Product>(),
            };
        }

        public static ProductLineDTO ToDTO(this ProductLine productLine)
        {
            return new ProductLineDTO
            {
                Id = productLine.Id,
                Name = productLine.Name,
                Description = productLine.Description ?? string.Empty,
                Image = productLine.Image ?? string.Empty,
                IsActive = productLine.IsActive,
                ManuallyDeactivated = productLine.ManuallyDeactivated,
                BrandName = productLine.Brand?.Name ?? string.Empty,
                Products = productLine.Products.Select(p => p.ToProductDTO()).ToHashSet()
            };
        }
    }
}