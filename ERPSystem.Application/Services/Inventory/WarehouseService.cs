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

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<WarehouseDto>> GetAllAsync(int? companyId = null, int? branchId = null)
        {
            var warehouses = await _warehouseRepository.GetAllAsync(companyId, branchId);

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
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
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
            var codeExists = await _warehouseRepository.CodeExistsAsync(dto.Code, dto.CompanyId);
            if (codeExists)
            {
                //// TODO use     custom exceptions | validation result
                throw new System.InvalidOperationException("Warehouse code already exists for this company.");
            }

            var warehouse = new Warehouse
            {
                CompanyId = dto.CompanyId,
                BranchId = dto.BranchId,
                Code = dto.Code,
                Name = dto.Name,
                Address = dto.Address,
                IsActive = true
            };

            await _warehouseRepository.AddAsync(warehouse);
            await _warehouseRepository.SaveChangesAsync();

            return warehouse.Id;
        }

        public async Task<bool> UpdateAsync(int id, UpdateWarehouseDto dto)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null) return false;

            var codeExists = await _warehouseRepository.CodeExistsAsync(dto.Code, warehouse.CompanyId, id);
            if (codeExists)
            {
                throw new System.InvalidOperationException("Warehouse code already exists for this company.");
            }

            warehouse.Code = dto.Code;
            warehouse.Name = dto.Name;
            warehouse.Address = dto.Address;
            warehouse.IsActive = dto.IsActive;

            await _warehouseRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse == null) return false;

            var hasActivity = await _warehouseRepository.HasInventoryActivityAsync(id);

            if (hasActivity)
            {
                // Mark as inactive only
                warehouse.IsActive = false;
            }
            else
            {
                // Hard delete or soft delete using IsDeleted
                warehouse.IsDeleted = true;
            }

            await _warehouseRepository.SaveChangesAsync();
            return true;
        }
    }
}
