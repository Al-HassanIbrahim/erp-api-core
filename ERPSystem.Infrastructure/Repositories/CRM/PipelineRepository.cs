using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories.CRM
{
    public class PipelineRepository:IPipelineRepository
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _current;

        private int CurrentCompanyId => _current.CompanyId;

        public PipelineRepository(AppDbContext db, ICurrentUserService current)
        {
            _db = db;
            _current = current;
        }

        private void Ensure(int companyId) => TenantGuard.EnsureCompany(companyId, CurrentCompanyId);

        private IQueryable<Pipeline> ScopedPipelines(int companyId)
        {
            Ensure(companyId);
            return _db.Pipelines.Where(x => x.CompanyId == companyId);
        }

        public Task<Pipeline?> GetByIdAsync(int id, int companyId, CancellationToken ct = default)
            => ScopedPipelines(companyId).FirstOrDefaultAsync(x => x.Id == id, ct);

        public async Task<List<Pipeline>> ListAsync(
            int companyId,
            CancellationToken ct = default)
        {
            IQueryable<Pipeline> q = ScopedPipelines(companyId).AsNoTracking();
            return await q
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task AddAsync(Pipeline pipeline, int companyId, CancellationToken ct = default)
        {
            Ensure(companyId);
            pipeline.CompanyId = companyId;

            await _db.Pipelines.AddAsync(pipeline, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Pipeline pipeline, int companyId, CancellationToken ct = default)
        {
            Ensure(companyId);

            if (pipeline.CompanyId != companyId)
                throw new UnauthorizedAccessException("Cross-company update is not allowed.");

            _db.Pipelines.Update(pipeline);
             await _db.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(int id, int companyId, CancellationToken ct = default)
        {
            Ensure(companyId);

            var entity = await _db.Pipelines.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);
            if (entity == null) return;

            _db.Pipelines.Remove(entity);
            await _db.SaveChangesAsync(ct);
        }

        public Task SaveChangesAsync(CancellationToken ct = default)
            => _db.SaveChangesAsync(ct);
    }
}
