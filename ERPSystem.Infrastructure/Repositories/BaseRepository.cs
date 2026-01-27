using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories
{
    public abstract class BaseRepository<TEntity>
        where TEntity : class, ICompanyEntity
    {
        protected readonly AppDbContext _context;
        protected readonly ICurrentUserService _current;
        protected int CompanyId => _current.CompanyId;

        protected BaseRepository(AppDbContext context, ICurrentUserService current)
        {
            _context = context;
            _current = current;
        }

        protected DbSet<TEntity> Set => _context.Set<TEntity>();

        // Returns a query scoped automatically to the current company
        public virtual IQueryable<TEntity> Query()
            => Set.Where(e => e.CompanyId == CompanyId);

        // Gets entity by Id, ensuring it belongs to the current company
        public virtual Task<TEntity?> GetByIdAsync(Guid id)
            => Query().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);

        public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync()
            => await Query().ToListAsync();

        public virtual async Task AddAsync(TEntity entity)
        {
            entity.CompanyId = CompanyId; // enforce company on insert
            await Set.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            if (entity.CompanyId != CompanyId)
                throw new UnauthorizedAccessException("Cross-company update is not allowed.");

            Set.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return;

            Set.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
