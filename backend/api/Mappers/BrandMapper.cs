using api.DTOs.Brand;
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
                Logo = brand.Logo
            };
        }

        // public static Brand ToModel(this CreateBrandDTO brandDTO)
        // {
        //     return new Brand
        //     {
        //         Name = brandDTO.Name,
        //     };
        // }

        public static Brand ToModel(this BrandDTO brandDTO)
        {
            return new Brand
            {
                Id = brandDTO.Id,
                Name = brandDTO.Name,
                Logo = brandDTO.Logo
            };
        }
    }
}