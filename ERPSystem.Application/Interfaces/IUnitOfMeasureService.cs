using ERPSystem.Application.DTOs.Products;

namespace ERPSystem.Application.Interfaces
{
    public interface IUnitOfMeasureService
    {
        Task<IReadOnlyList<UnitOfMeasureDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UnitOfMeasureDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<UnitOfMeasureDto> CreateAsync(CreateUnitOfMeasureRequest request, CancellationToken cancellationToken = default);
        Task<UnitOfMeasureDto> UpdateAsync(int id, UpdateUnitOfMeasureRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    }
}