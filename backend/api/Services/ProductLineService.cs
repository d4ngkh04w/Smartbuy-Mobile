using api.DTOs.ProductLine;
using api.Interfaces.Repositories;
using api.Interfaces.Services;
using api.Mappers;
using api.Models;
using api.Queries;

namespace api.Services
{
    public class ProductLineService : IProductLineService
    {
        private readonly IProductLineRepository _productLineRepository;

        public ProductLineService(IProductLineRepository productLineRepository)
        {
            _productLineRepository = productLineRepository;
        }

        public async Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> CreateProductLineAsync(CreateProductLineDTO productLineDTO)
        {
            try
            {
                bool exists = await _productLineRepository.ProductLineExistAsync(productLineDTO.Name);
                if (exists)
                {
                    return (false, "Product line already exists", null);
                }

                ProductLine productLine = productLineDTO.ToModel();
                var createdProductLine = await _productLineRepository.CreateProductLineAsync(productLine);
                
                return (true, null, createdProductLine.ToDTO());
            }
            catch (Exception ex)
            {
                return (false, $"Error creating product line: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteProductLineAsync(int id)
        {
            try
            {
                var productLine = await _productLineRepository.GetProductLineByIdAsync(id);
                if (productLine == null)
                {
                    return (false, "Product line not found");
                }

                await _productLineRepository.DeleteProductLineAsync(productLine);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting product line: {ex.Message}");
            }
        }

        public async Task<(bool Success, string? ErrorMessage, ProductLineDTO? ProductLine)> GetProductLineByIdAsync(int id, ProductLineQuery? query = null)
        {
            try
            {
                var productLine = await _productLineRepository.GetProductLineByIdAsync(id, query);
                if (productLine == null)
                {
                    return (false, "Product line not found", null);
                }
                
                return (true, null, productLine.ToDTO());
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving product line: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, IEnumerable<ProductLineDTO>? ProductLines)> GetProductLinesAsync(ProductLineQuery query)
        {
            try
            {
                var productLines = await _productLineRepository.GetProductLinesAsync(query);
                if (productLines == null || !productLines.Any())
                {
                    return (false, "No product lines found", null);
                }
                
                return (true, null, productLines.Select(pl => pl.ToDTO()));
            }
            catch (Exception ex)
            {
                return (false, $"Error retrieving product lines: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateProductLineAsync(int id, UpdateProductLineDTO productLineDTO)
        {
            try
            {
                var productLine = await _productLineRepository.GetProductLineByIdAsync(id);
                if (productLine == null)
                {
                    return (false, "Product line not found");
                }

                // Only update fields that are provided in the DTO
                if (!string.IsNullOrEmpty(productLineDTO.Name))
                {
                    productLine.Name = productLineDTO.Name;
                }

                if (!string.IsNullOrEmpty(productLineDTO.Description))
                {
                    productLine.Description = productLineDTO.Description;
                }

                if (!string.IsNullOrEmpty(productLineDTO.Image))
                {
                    productLine.Image = productLineDTO.Image;
                }

                if (productLineDTO.IsActive.HasValue)
                {
                    productLine.IsActive = productLineDTO.IsActive.Value;
                }

                if (productLineDTO.BrandId.HasValue)
                {
                    productLine.BrandId = productLineDTO.BrandId.Value;
                }

                productLine.UpdatedAt = DateTime.Now;

                bool result = await _productLineRepository.UpdateProductLineAsync(productLine);
                if (!result)
                {
                    return (false, "Failed to update product line");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating product line: {ex.Message}");
            }
        }
    }
}