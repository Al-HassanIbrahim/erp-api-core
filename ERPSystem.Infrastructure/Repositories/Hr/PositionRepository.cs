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

        public async Task<JobPosition?> GetByIdAsync(Guid id)
        {
            return await Query()
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<JobPosition>> GetAllAsync()
        {
            return await Query()
                .Include(p => p.Department)
                .ToListAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string code)
        {
            return await Query()
                .AnyAsync(p => p.Code == code);
        }

        // CRUD: delegate to base (enforces CompanyId + blocks cross-company updates)
        public Task AddAsync(JobPosition position) => base.AddAsync(position);
        public Task UpdateAsync(JobPosition position) => base.UpdateAsync(position);
        public Task DeleteAsync(Guid id) => base.DeleteAsync(id);
    }
}
