using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.HR;
using ERPSystem.Infrastructure.Data;
using ERPSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Hr
{
    public class PositionRepository : BaseRepository<JobPosition>, IPositionRepository
    {
        public PositionRepository(AppDbContext context, ICurrentUserService current)
            : base(context, current) { }

        private void EnsureCompany(int companyId)
        {
            if (companyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company access is not allowed.");
        }

        public async Task<JobPosition?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        public async Task<IEnumerable<JobPosition>> GetAllAsync(int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .Include(p => p.Department)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsByCodeAsync(string code, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);

            return await Query()
                .AnyAsync(p => p.Code == code, ct);
        }

        public Task AddAsync(JobPosition position) => base.AddAsync(position);
        public Task UpdateAsync(JobPosition position) => base.UpdateAsync(position);

        public async Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default)
        {
            EnsureCompany(companyId);
            await base.DeleteAsync(id);
        }
    }
}
