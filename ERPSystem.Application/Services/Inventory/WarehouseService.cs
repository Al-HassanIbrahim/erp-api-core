using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Inventory;

namespace ERPSystem.Application.Services.Inventory
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;

        public WarehouseService(
            IWarehouseRepository warehouseRepository,
            ICurrentUserService currentUser)
        {
            _warehouseRepository = warehouseRepository;
            _currentUser = currentUser;
        }

        public async Task<List<WarehouseDto>> GetAllAsync(int? branchId = null)
        {
            var warehouses = await _warehouseRepository.GetAllAsync(_currentUser.CompanyId, branchId);

            return warehouses
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Code = w.Code,
                    Name = w.Name,
                    Address = w.Address,
                    IsActive = w.IsActive
                })
                .ToList();
        }

        public async Task<WarehouseDto?> GetByIdAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id, _currentUser.CompanyId);
            if (warehouse == null) return null;

            return new WarehouseDto
            {
                Id = warehouse.Id,
                Code = warehouse.Code,
                Name = warehouse.Name,
                Address = warehouse.Address,
                IsActive = warehouse.IsActive
            };
        }

        public async Task<int> CreateAsync(CreateWarehouseDto dto)
        {
            var codeExists = await _warehouseRepository.CodeExistsAsync(dto.Code, _currentUser.CompanyId);
            if (codeExists)
                throw new InvalidOperationException("Warehouse code already exists for this company.");

            var warehouse = new Warehouse
            {
                CompanyId = _currentUser.CompanyId,
                BranchId = dto.BranchId,
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveChangesAsync();

            return warehouse.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateWarehouseDto dto)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id, _currentUser.CompanyId);
            if (warehouse == null) return false;

            var codeExists = await _warehouseRepository.CodeExistsAsync(dto.Code, _currentUser.CompanyId, id);
            if (codeExists)
                throw new InvalidOperationException("Warehouse code already exists for this company.");

            warehouse.Code = dto.Code;
            warehouse.Name = dto.Name;
            warehouse.Address = dto.Address;
            warehouse.IsActive = dto.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;

            await _warehouseRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id, _currentUser.CompanyId);
            if (warehouse == null) return false;

            var hasActivity = await _warehouseRepository.HasInventoryActivityAsync(id);

            if (hasActivity)
            {
                warehouse.IsActive = false;
            }
            else
            {
                warehouse.IsDeleted = true;
            }

            warehouse.UpdatedAt = DateTime.UtcNow;
            await _warehouseRepository.SaveChangesAsync();
            return true;
        }
    }
}
