using api.DTOs.Brand;
using api.DTOs.ProductLine;
using api.Models;

namespace api.Mappers
{
    public static class BrandMapper
    {
        public static BrandDTO ToDTO(this Brand brand)
        {
            return new BrandDTO
            {
                Id = brand.Id,
                Name = brand.Name,
                Logo = brand.Logo,
                Description = brand.Description,
                IsActive = brand.IsActive,
                ProductLines = brand.ProductLines?.Select(pl => pl.ToDTO()).ToList()
            };
        }
    }
}