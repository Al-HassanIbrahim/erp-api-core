using ERPSystem.Application.DTOs.CRM;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.Application.Services.CRM
{
    public class PipelineService : IPipelineService
    {
        private readonly IPipelineRepository _pipelineRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly ILeadRepository _leadRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ICurrentUserService _current;
        private readonly IModuleAccessService _moduleAccess;

        public PipelineService(
            IPipelineRepository pipelineRepo,
            ICustomerRepository customerRepo,
            ILeadRepository leadRepo,
            IEmployeeRepository employeeRepo,
            ICurrentUserService current,
            IModuleAccessService moduleAccess)
        {
            _pipelineRepo = pipelineRepo;
            _customerRepo = customerRepo;
            _leadRepo = leadRepo;
            _employeeRepo = employeeRepo;
            _current = current;
            _moduleAccess = moduleAccess;
        }

        // ================== GUARDS ==================

        private void EnsureCompany(int companyId)
        {
            if (companyId != _current.CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        private async Task<Pipeline> GetValidPipelineAsync(int id, int companyId, CancellationToken ct = default)
        {
            var pipe = await _pipelineRepo.GetByIdAsync(id, companyId, ct);
            if (pipe == null)
                throw new InvalidOperationException("Pipeline not found or does not belong to your company.");
            return pipe;
        }

        private async Task EnsureCustomerExistsAsync(int customerId, int companyId, CancellationToken ct = default)
        {
            // IMPORTANT: لازم تكون company-scoped
            var customer = await _customerRepo.GetByIdAsync(customerId, ct);
            if (customer == null)
                throw new InvalidOperationException("Customer not found or does not belong to your company.");
        }

        private async Task EnsureEmployeeExistsAsync(Guid employeeId, int companyId, CancellationToken ct = default)
        {
            var emp = await _employeeRepo.GetByIdAsync(employeeId, companyId, ct);
            if (emp == null)
                throw new InvalidOperationException("Employee not found or does not belong to your company.");
        }

        private async Task EnsureLeadExistsAsync(int leadId, int companyId, CancellationToken ct = default)
        {
            var lead = await _leadRepo.GetByIdAsync(leadId, companyId, ct);
            if (lead == null)
                throw new InvalidOperationException("Lead not found or does not belong to your company.");
        }

        // ================== MAPPERS ==================
        // (لو الـ PipelineDto عندك فيه حقول أكتر/أقل عدّل المابنج هنا فقط)

        private static PipelineDto MapToDto(Pipeline x) => new PipelineDto
        {
            Id = x.Id,
            CompanyId = x.CompanyId,
            DealName = x.DealName,
            CustomerId = x.CustomerId,
            LeadId = x.LeadId,
            DealAmount = x.Amount,
            ExpectedCloseDate = x.ExpectedCloseDate,
            OwnerId = x.OwnerId,
            DealStage = x.Stage,             
        };

        private static void ApplyCreate(Pipeline entity, CreatePipelineDto dto)
        {
            entity.DealName = dto.DealName.Trim();
            entity.CustomerId = dto.CustomerId;
            entity.LeadId = dto.LeadId;
            entity.Amount = dto.DealAmount;
            entity.ExpectedCloseDate = dto.ExpectedCloseDate;
            entity.OwnerId = dto.OwnerId;
            entity.Stage = dto.DealStage;
        }

        private static void ApplyUpdate(Pipeline entity, UpdatePipelineDto dto)
        {
            entity.DealName = dto.DealName.Trim();
            entity.CustomerId = dto.CustomerId;
            entity.LeadId = dto.LeadId;
            entity.Amount = dto.DealAmount;
            entity.ExpectedCloseDate = dto.ExpectedCloseDate;
            entity.OwnerId = dto.OwnerId;
            entity.Stage = dto.DealStage;
        }

        // ================== READ ==================

        public async Task<PipelineDto?> GetByIdAsync(int id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            var pipe = await _pipelineRepo.GetByIdAsync(id, companyId, ct);
            return pipe == null ? null : MapToDto(pipe);
        }

        public async Task<List<PipelineDto>> ListAsync(int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            var list = await _pipelineRepo.ListAsync(
                companyId:companyId,
                ct: ct);

            return list.Select(MapToDto).ToList();
        }

        // ================== CREATE ==================

        public async Task<int> CreateAsync(CreatePipelineDto dto, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            if (string.IsNullOrWhiteSpace(dto.DealName))
                throw new InvalidOperationException("Deal name is required.");

            if (dto.DealAmount < 0)
                throw new InvalidOperationException("Amount cannot be negative.");

            // Relations (company scoped)
            await EnsureCustomerExistsAsync(dto.CustomerId, companyId, ct);

            if (dto.LeadId.HasValue)
                await EnsureLeadExistsAsync(dto.LeadId.Value, companyId, ct);

            if (dto.OwnerId.HasValue)
                await EnsureEmployeeExistsAsync(dto.OwnerId.Value, companyId, ct);

            var pipe = new Pipeline
            {
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            };

            ApplyCreate(pipe, dto);

            await _pipelineRepo.AddAsync(pipe, companyId, ct);
            await _pipelineRepo.SaveChangesAsync(ct);

            return pipe.Id;
        }

        // ================== UPDATE ==================

        public async Task UpdateAsync(int id, UpdatePipelineDto dto, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            var pipe = await GetValidPipelineAsync(id, companyId, ct);

            if (string.IsNullOrWhiteSpace(dto.DealName))
                throw new InvalidOperationException("Deal name is required.");

            if (dto.DealAmount < 0)
                throw new InvalidOperationException("Amount cannot be negative.");

            await EnsureCustomerExistsAsync(dto.CustomerId, companyId, ct);

            if (dto.LeadId.HasValue)
                await EnsureLeadExistsAsync(dto.LeadId.Value, companyId, ct);

            if (dto.OwnerId.HasValue)
                await EnsureEmployeeExistsAsync(dto.OwnerId.Value, companyId, ct);

            ApplyUpdate(pipe, dto);

            pipe.UpdatedAt = DateTime.UtcNow;

            await _pipelineRepo.UpdateAsync(pipe, companyId);
            await _pipelineRepo.SaveChangesAsync(ct);
        }

        // ================== DELETE ==================

        public async Task DeleteAsync(int id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            // guard existence + scope
            _ = await GetValidPipelineAsync(id, companyId, ct);

            await _pipelineRepo.DeleteAsync(id, companyId, ct);
            await _pipelineRepo.SaveChangesAsync(ct);
        }

        // ================== MOVE STAGE ==================

        public async Task MoveStageAsync(int id, MovePiplineStageDto dto, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await _moduleAccess.EnsureCrmEnabledAsync(ct);

            var pipe = await GetValidPipelineAsync(id, companyId, ct);
            if (dto == null)
                throw new InvalidOperationException("Move stage payload is required.");

            pipe.Stage = dto.Stage; 

            pipe.UpdatedAt = DateTime.UtcNow;

            await _pipelineRepo.UpdateAsync(pipe, companyId);
            await _pipelineRepo.SaveChangesAsync(ct);
        }

        
    }
}
