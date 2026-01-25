using ERPSystem.Application.DTOs;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Application.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICurrentUserService _currentUser;

        public ProductService(IProductRepository productRepository, ICurrentUserService currentUser)
        {
            _productRepository = productRepository;
            _currentUser = currentUser;
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id, _currentUser.CompanyId);

            if (product == null)
                return null;

            return MapToDto(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllByCompanyAsync(_currentUser.CompanyId);
            return products.Select(MapToDto).ToList();
        }

        public async Task<int> CreateAsync(CreateProductDto dto)
        {
            var codeExists = await _productRepository.CodeExistsAsync(dto.Code, _currentUser.CompanyId);
            if (codeExists)
                throw new InvalidOperationException($"Product code '{dto.Code}' already exists.");

            var product = new Product
            {
                CompanyId = _currentUser.CompanyId,
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
            await _productRepository.SaveChangesAsync();
            
            return product.Id;
        }

        public async Task UpdateAsync(UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.Id, _currentUser.CompanyId);

            if (product == null)
                throw new InvalidOperationException($"Product with ID {dto.Id} not found or does not belong to your company.");

            var codeExists = await _productRepository.CodeExistsAsync(dto.Code, _currentUser.CompanyId, dto.Id);
            if (codeExists)
                throw new InvalidOperationException($"Product code '{dto.Code}' already exists.");

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
            await _productRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id, _currentUser.CompanyId);

            if (product == null)
                return;

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _productRepository.SaveChangesAsync();
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
