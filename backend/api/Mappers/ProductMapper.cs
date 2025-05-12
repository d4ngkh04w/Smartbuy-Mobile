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
                Quantity = productColor.Quantity,
                Images = productColor.Images?.Select(i => i.ToProductImageDTO()).ToHashSet() ?? new HashSet<ProductImageDTO>(),
                // HasMainImage = productColor.Images?.Any(i => i.IsMain) ?? false
            };
        }

        public static ProductImageDTO ToProductImageDTO(this ProductImage productImage)
        {
            return new ProductImageDTO
            {
                Id = productImage.Id,
                ImagePath = productImage.ImagePath,
                IsMain = productImage.IsMain
            };
        }

        public static ProductDetailDTO ToProductDetailDTO(this ProductDetail productDetail)
        {
            return new ProductDetailDTO
            {
                Id = productDetail.Id,
                Warranty = productDetail.WarrantyMonths.ToString(),
                RAM = productDetail.RAMInGB.ToString(),
                Storage = productDetail.StorageInGB.ToString(),
                Processor = productDetail.Processor,
                OperatingSystem = productDetail.OperatingSystem,
                ScreenSize = productDetail.ScreenSizeInch.ToString(),
                Battery = productDetail.BatteryCapacityMAh.ToString(),
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
                Stock = product.Colors.Sum(c => c.Quantity),
                ImportPrice = product.ImportPrice,
                SalePrice = product.SalePrice,
                Description = product.Description ?? string.Empty,
                Rating = product.Rating,
                RatingCount = product.RatingCount,
                Sold = product.Sold,
                IsActive = product.IsActive,
                ManuallyDeactivated = product.ManuallyDeactivated,
                CreatedAt = DateTime.Parse(product.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")),
                UpdatedAt = product.UpdatedAt.HasValue ? DateTime.Parse(product.UpdatedAt.Value.ToString("yyyy-MM-ddTHH:mm:ss")) : DateTime.Now,
                ProductLineId = product.ProductLineId,
                ProductLineName = product.ProductLine?.Name ?? string.Empty,

                Colors = product.Colors.Select(c => c.ToProductColorDTO()).ToHashSet(),
                Detail = product.Detail?.ToProductDetailDTO(),
                // Add tags when implementing tag functionality
                // Tags = product.ProductTags.Select(pt => pt.Tag).ToHashSet()
            };
        }

        public static Product ToProductModel(this CreateProductDTO productDTO)
        {
            return new Product
            {
                Name = productDTO.Name,
                ImportPrice = productDTO.ImportPrice,
                SalePrice = productDTO.SalePrice,
                Description = productDTO.Description,
                ProductLineId = productDTO.ProductLineId,
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
                WarrantyMonths = productDTO.Warranty,
                RAMInGB = productDTO.RAM,
                StorageInGB = productDTO.Storage,
                Processor = productDTO.Processor,
                OperatingSystem = productDTO.OS,
                ScreenSizeInch = productDTO.ScreenSize,
                BatteryCapacityMAh = productDTO.Battery,
                SimSlots = productDTO.SimSlots,
                ScreenResolution = productDTO.ScreenResolution,
            };
        }
        public static ProductSummaryDTO ToSummaryDTO(this Product product)
        {
            string mainImagePath = string.Empty;
            foreach (var color in product.Colors)
            {
                var mainImage = color.Images.FirstOrDefault(i => i.IsMain);
                if (mainImage != null)
                {
                    mainImagePath = mainImage.ImagePath;
                    break;
                }
            }

            return new ProductSummaryDTO
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.SalePrice,
                ImageUrl = mainImagePath,
                Rating = product.Rating,
                RatingCount = product.RatingCount,
                Sold = product.Sold,
            };
        }

    }
}