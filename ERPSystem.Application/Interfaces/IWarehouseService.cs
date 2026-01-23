using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Inventory;

namespace ERPSystem.Application.Interfaces
{
    public interface IWarehouseService
    {
        Task<List<WarehouseDto>> GetAllAsync(int? companyId = null, int? branchId = null);
        Task<WarehouseDto?> GetByIdAsync(int id);

        Task<int> CreateAsync(CreateWarehouseDto dto);

        Task<bool> UpdateAsync(int id, UpdateWarehouseDto dto);

        // soft delete (deactivate)
        Task<bool> DeleteAsync(int id);
    }
}
