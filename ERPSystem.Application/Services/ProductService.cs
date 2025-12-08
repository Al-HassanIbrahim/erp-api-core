

using ERPSystem.Application.DTOs;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null || product.IsDeleted)
                return null;

            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();

            return products
                .Where(p => !p.IsDeleted)
                .Select(MapToDto)
                .ToList();
        }

        public async Task<int> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                UnitOfMeasureId = dto.UnitOfMeasureId,
                DefaultPrice = dto.DefaultPrice,
                Barcode = dto.Barcode,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            return product.Id;
        }

        public async Task UpdateAsync(UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.Id);

            if (product == null || product.IsDeleted)
                throw new KeyNotFoundException($"Product with ID {dto.Id} not found.");

            product.Code = dto.Code;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;
            product.UnitOfMeasureId = dto.UnitOfMeasureId;
            product.DefaultPrice = dto.DefaultPrice;
            product.Barcode = dto.Barcode;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return;

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                DefaultPrice = product.DefaultPrice,
                CategoryName = product.Category?.Name,
                UnitOfMeasureName = product.UnitOfMeasure?.Name ?? string.Empty,
                IsActive = product.IsActive
            };
        }
    }

}
