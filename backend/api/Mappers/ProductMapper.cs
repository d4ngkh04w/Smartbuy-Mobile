using api.DTOs.Product;
using api.Interfaces.Repositories;
using api.Models;
using api.Utils;

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
                Rating = 0,
                RatingCount = 0,
                Description = product.Description ?? string.Empty,
                Sold = product.Sold,
                IsActive = product.IsActive,
                ManuallyDeactivated = product.ManuallyDeactivated,
                CreatedAt = DateTimeUtils.FormatDateTime(product.CreatedAt),
                UpdatedAt = product.UpdatedAt.HasValue ? DateTimeUtils.FormatDateTime(product.UpdatedAt.Value) : DateTimeUtils.FormatDateTime(DateTime.Now),
                ProductLineId = product.ProductLineId,
                ProductLineName = product.ProductLine?.Name ?? string.Empty,
                Discounts = product.Discounts.Select(d => d.Discount!.ToDTO()).ToHashSet(),
                Colors = product.Colors.Select(c => c.ToProductColorDTO()).ToHashSet(),
                Detail = product.Detail?.ToProductDetailDTO(),
                Price = product.Discounts
                    .Select(d => d.Discount!.CalculateDiscountedPrice(product.SalePrice))
                    .DefaultIfEmpty(product.SalePrice)
                    .Min(),
                Discount = product.Discounts
                    .OrderBy(d => d.Discount!.CalculateDiscountedPrice(product.SalePrice)) // sắp theo giá sau giảm (giảm càng nhiều thì giá càng thấp)
                    .Select(d => d.Discount!.ToString())
                    .FirstOrDefault() ?? string.Empty,

                // Add tags when implementing tag functionality
                // Tags = product.ProductTags.Select(pt => pt.Tag).ToHashSet()
            };
        }

        public static async Task<ProductDTO> ToProductDTO(this Product product, ICommentRepository commentRepository)
        {
            return new ProductDTO
            {
                Rating = (decimal)await commentRepository.GetProductAverageRatingAsync(product.Id),
                RatingCount = await commentRepository.GetProductRatingCountAsync(product.Id),
                Id = product.Id,
                Name = product.Name,
                Stock = product.Colors.Sum(c => c.Quantity),
                ImportPrice = product.ImportPrice,
                SalePrice = product.SalePrice,
                Description = product.Description ?? string.Empty,
                Sold = product.Sold,
                IsActive = product.IsActive,
                ManuallyDeactivated = product.ManuallyDeactivated,
                CreatedAt = DateTimeUtils.FormatDateTime(product.CreatedAt),
                UpdatedAt = product.UpdatedAt.HasValue ? DateTimeUtils.FormatDateTime(product.UpdatedAt.Value) : DateTimeUtils.FormatDateTime(DateTime.Now),
                ProductLineId = product.ProductLineId,
                ProductLineName = product.ProductLine?.Name ?? string.Empty,
                Discounts = product.Discounts.Select(d => d.Discount!.ToDTO()).ToHashSet(),
                Colors = product.Colors.Select(c => c.ToProductColorDTO()).ToHashSet(),
                Detail = product.Detail?.ToProductDetailDTO(),
                Price = product.Discounts
                    .Select(d => d.Discount!.CalculateDiscountedPrice(product.SalePrice))
                    .DefaultIfEmpty(product.SalePrice)
                    .Min(),
                Discount = product.Discounts
                    .OrderBy(d => d.Discount!.CalculateDiscountedPrice(product.SalePrice)) // sắp theo giá sau giảm (giảm càng nhiều thì giá càng thấp)
                    .Select(d => d.Discount!.ToString())
                    .FirstOrDefault() ?? string.Empty,
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
                CreatedAt = DateTimeUtils.FormatDateTime(DateTime.Now),
                UpdatedAt = DateTimeUtils.FormatDateTime(DateTime.Now),
                IsActive = true,
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
        }        public static async Task<ProductSummaryDTO> ToSummaryDTO(this Product product, ICommentRepository commentRepository)
        {
            return new ProductSummaryDTO
            {
                Rating = (decimal)await commentRepository.GetProductAverageRatingAsync(product.Id),
                RatingCount = await commentRepository.GetProductRatingCountAsync(product.Id),
                Id = product.Id,
                Name = product.Name,
                Price = product.Discounts
                    .Select(d => d.Discount!.CalculateDiscountedPrice(product.SalePrice))
                    .DefaultIfEmpty(product.SalePrice)
                    .Min(), // Giá sau khi giảm (thấp nhất)
                SalePrice = product.SalePrice, // Giá gốc
                Discount = product.Discounts
                    .OrderBy(d => d.Discount!.CalculateDiscountedPrice(product.SalePrice))
                    .Select(d => d.Discount!.ToString())
                    .FirstOrDefault() ?? string.Empty, // Discount có giá thấp nhất
                ImageUrl = product.Colors
                    .SelectMany(c => c.Images)
                    .FirstOrDefault(i => i.IsMain)?.ImagePath ?? string.Empty,
                Sold = product.Sold,
            };
        }

    }
}