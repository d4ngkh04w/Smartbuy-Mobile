using api.DTOs.Product;
using api.Models;

namespace api.Mappers
{
    public static class ProductMapper
    {
        public static ProductColorDTO ToProductColorDTO(this ProductColor productColor)
        {

            return new ProductColorDTO
            {
                Id = productColor.Id,
                Name = productColor.Name,
            };
        }

        public static ProductImageDTO ToProductImageDTO(this ProductImage productImage)
        {
            return new ProductImageDTO
            {
                Id = productImage.Id,
                ImagePath = productImage.ImagePath,
                IsMain = productImage.IsMain,
            };
        }

        public static ProductDetailDTO ToProductDetailDTO(this ProductDetail productDetail)
        {
            return new ProductDetailDTO
            {
                Id = productDetail.Id,
                Warranty = productDetail.Warranty,
                RAM = productDetail.RAM,
                Storage = productDetail.Storage,
                Processor = productDetail.Processor,
                ScreenSize = productDetail.ScreenSize,
                Battery = productDetail.Battery,
                SimSlots = productDetail.SimSlots,
                ScreenResolution = productDetail.ScreenResolution,
            };
        }

        public static ProductDTO ToProductDTO(this Product product)
        {
            return new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Quantity = product.Quantity,
                SalePrice = product.SalePrice,
                Description = product.Description,
                Rating = product.Rating,
                RatingCount = product.RatingCount,
                Sold = product.Sold,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,

                Colors = product.Colors.Select(c => c.ToProductColorDTO()).ToHashSet(),
                Images = product.Images.Select(i => i.ToProductImageDTO()).ToHashSet(),
                Detail = product.Detail?.ToProductDetailDTO(),
            };
        }

        public static Product ToProductModel(this CreateProductDTO productDTO)
        {
            return new Product
            {
                Name = productDTO.Name,
                Quantity = productDTO.Quantity,
                ImportPrice = productDTO.ImportPrice,
                SalePrice = productDTO.SalePrice,
                Description = productDTO.Description,
                CategoryId = productDTO.CategoryId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true,
                Rating = 0,
                RatingCount = 0,
                Sold = 0,

            };
        }
        public static ProductDetail ToProductDetailModel(this CreateProductDTO productDTO)
        {
            return new ProductDetail
            {
                Warranty = productDTO.Warranty,
                RAM = productDTO.RAM,
                Storage = productDTO.Storage,
                Processor = productDTO.Processor,
                ScreenSize = productDTO.ScreenSize,
                Battery = productDTO.Battery,
                SimSlots = productDTO.SimSlots,
                ScreenResolution = productDTO.ScreenResolution,
            };
        }
    }

}