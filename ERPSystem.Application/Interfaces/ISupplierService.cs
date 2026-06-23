using ERPSystem.Application.DTOs.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<IReadOnlyList<SupplierListDto>> GetAllAsync(CancellationToken ct = default);
        Task<SupplierDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<SupplierDto> CreateAsync(CreateSupplierDto request, CancellationToken ct = default);
        Task<SupplierDto> UpdateAsync(int id, UpdateSupplierDto request, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);
    }
}
