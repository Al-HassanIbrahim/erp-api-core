using ERPSystem.Application.DTOs.CRM;
using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface ILeadService
    {
        Task<LeadDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<List<LeadDto>> ListAsync(CancellationToken ct = default);

        Task<int> CreateAsync(CreateLeadDto dto, string createdBy, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdateLeadDto dto, CancellationToken ct = default);
        Task DeleteAsync(int id, CancellationToken ct = default);

        Task ConvertAsync(int leadId, ConvertLeadDto dto, string modifiedBy, CancellationToken ct = default);
    }
}
