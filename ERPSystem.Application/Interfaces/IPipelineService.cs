using ERPSystem.Application.DTOs.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface IPipelineService
    {
        Task<PipelineDto?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);
        Task<List<PipelineDto>> ListAsync(int companyId,CancellationToken ct = default);

        Task<int> CreateAsync(CreatePipelineDto dto, int companyId, CancellationToken ct = default);
        Task UpdateAsync(int id, UpdatePipelineDto dto, int companyId, CancellationToken ct = default);
        Task DeleteAsync(int id, int companyId, CancellationToken ct = default);

        Task MoveStageAsync(int id, MovePiplineStageDto dto, int companyId, CancellationToken ct = default);
    }
}
