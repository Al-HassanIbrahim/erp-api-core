using ERPSystem.Application.DTOs.CRM;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.Application.Services.CRM
{
    public class LeadService : ILeadService
    {
        private readonly ILeadRepository _leadRepo;
        private readonly IPipelineRepository _pipelineRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ICurrentUserService _current;
        private readonly IModuleAccessService _moduleAccess;

        public LeadService(
            ILeadRepository leadRepo,
            IPipelineRepository pipelineRepo,
            IEmployeeRepository employeeRepo,
            ICustomerRepository customerRepo,
            ICurrentUserService current,
            IModuleAccessService moduleAccess)
        {
            _leadRepo = leadRepo;
            _pipelineRepo = pipelineRepo;
            _employeeRepo = employeeRepo;
            _customerRepo = customerRepo;
            _current = current;
            _moduleAccess = moduleAccess;
        }

        // ================== GUARDS ==================

        private int CompanyId => _current.CompanyId;

        private async Task<Lead> GetValidLeadAsync(int id, CancellationToken ct = default)
        {
            var lead = await _leadRepo.GetByIdAsync(id, CompanyId, ct);
            if (lead == null)
                throw new InvalidOperationException("Lead not found or does not belong to your company.");
            return lead;
        }

        private async Task EnsureEmployeeExistsAsync(Guid employeeId, CancellationToken ct = default)
        {

            var emp = await _employeeRepo.GetByIdAsync(employeeId, CompanyId, ct);
            if (emp == null)
                throw new InvalidOperationException("Assigned employee not found or does not belong to your company.");
        }

        private async Task EnsureCustomerExistsAsync(int customerId, CancellationToken ct = default)
        {
            var customer = await _customerRepo.GetByIdAsync(customerId, ct);
            if (customer == null)
                throw new InvalidOperationException("Customer not found or does not belong to your company.");
        }

        // ================== MAPPERS ==================

        private static LeadDto MapToDto(Lead x) => new LeadDto
        {
            Id = x.Id,
            CompanyId = x.CompanyId,
            Name = x.Name,
            CompanyName = x.CompanyName,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            Source = x.Source,
            Stage = x.Stage,
            DealValue = x.DealValue,
            LastContactDate = x.LastContactDate,
            AssignedToId = x.AssignedToId,
            ConvertedCustomerId = x.ConvertedCustomerId,
            ConvertedDate = x.ConvertedDate,
        };

        private static void ApplyCreate(Lead entity, CreateLeadDto dto)
        {
            entity.Name = dto.Name.Trim();
            entity.CompanyName = dto.CompanyName.Trim();
            entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            entity.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
            entity.Source = dto.Source;
            entity.Stage = dto.Stage;
            entity.DealValue = dto.DealValue;
            entity.LastContactDate = dto.LastContactDate;
            entity.AssignedToId = dto.AssignedToId;
        }

        private static void ApplyUpdate(Lead entity, UpdateLeadDto dto)
        {
            entity.Name = dto.Name.Trim();
            entity.CompanyName = dto.CompanyName.Trim();
            entity.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            entity.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
            entity.Source = dto.Source;
            entity.Stage = dto.Stage;
            entity.DealValue = dto.DealValue;
            entity.LastContactDate = dto.LastContactDate;
            entity.AssignedToId = dto.AssignedToId;
        }

        // ================== READ ==================

        public async Task<LeadDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureCrmEnabledAsync(ct); 
            var lead = await _leadRepo.GetByIdAsync(id, CompanyId, ct);
            return lead == null ? null : MapToDto(lead);
        }

        public async Task<List<LeadDto>> ListAsync(CancellationToken ct = default)
        {
            await _moduleAccess.EnsureCrmEnabledAsync(ct);
            var leads = await _leadRepo.ListAsync(
                CompanyId,ct: ct);

            return leads.Select(MapToDto).ToList();
        }

        // ================== CREATE ==================

        public async Task<int> CreateAsync(CreateLeadDto dto, string createdBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new InvalidOperationException("Lead name is required.");

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                throw new InvalidOperationException("Company name is required.");

            if (dto.DealValue.HasValue && dto.DealValue.Value < 0)
                throw new InvalidOperationException("Deal value cannot be negative.");

            if (dto.AssignedToId.HasValue)
                await EnsureEmployeeExistsAsync(dto.AssignedToId.Value, ct);

            var lead = new Lead
            {
                CompanyId = CompanyId,
                CreatedAt = DateTime.UtcNow
            };

            ApplyCreate(lead, dto);

            await _leadRepo.AddAsync(lead,CompanyId, ct);
            await _leadRepo.SaveChangesAsync(ct);
            return lead.Id;
        }

        // ================== UPDATE ==================

        public async Task UpdateAsync(int id, UpdateLeadDto dto, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            var lead = await GetValidLeadAsync(id, ct);

            if (dto.AssignedToId.HasValue)
                await EnsureEmployeeExistsAsync(dto.AssignedToId.Value, ct);

            if (dto.DealValue.HasValue && dto.DealValue.Value < 0)
                throw new InvalidOperationException("Deal value cannot be negative.");

            ApplyUpdate(lead, dto);

            await _leadRepo.UpdateAsync(lead,CompanyId);
        }

        // ================== DELETE ==================

        public async Task DeleteAsync(int id, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            var lead = await GetValidLeadAsync(id, ct);

            if (lead.ConvertedCustomerId.HasValue)
                throw new InvalidOperationException("Cannot delete a converted lead.");

            await _leadRepo.DeleteAsync(id, CompanyId, ct);
        }

        // ================== CONVERT ==================

        public async Task ConvertAsync(int leadId, ConvertLeadDto dto, string modifiedBy, CancellationToken ct = default)
        {
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            var lead = await GetValidLeadAsync(leadId, ct);

            if (lead.ConvertedCustomerId.HasValue)
                throw new InvalidOperationException("Lead is already converted.");

            await EnsureCustomerExistsAsync(dto.CustomerId, ct);

            if (dto.OwnerId.HasValue)
                await EnsureEmployeeExistsAsync(dto.OwnerId.Value, ct);

            // Update lead conversion
            lead.ConvertedCustomerId = dto.CustomerId;
            lead.ConvertedDate = DateTime.UtcNow;

            await _leadRepo.UpdateAsync(lead, CompanyId);

            // Optional: create deal/pipeline
            if (dto.CreateDeal)
            {
                var deal = new Pipeline
                {
                    CompanyId = CompanyId,
                    DealName = string.IsNullOrWhiteSpace(dto.DealName)
                        ? $"{lead.CompanyName} - Deal"
                        : dto.DealName.Trim(),

                    CustomerId = dto.CustomerId,
                    LeadId = lead.Id,

                    Amount = dto.DealAmount ?? lead.DealValue ?? 0m,
                    ExpectedCloseDate = dto.ExpectedCloseDate,
                    OwnerId = dto.OwnerId ?? lead.AssignedToId,
                };

                await _pipelineRepo.AddAsync(deal, CompanyId,ct);
            }
        }
    }
}
