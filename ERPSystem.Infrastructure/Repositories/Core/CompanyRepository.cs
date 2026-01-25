using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Core
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Company?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public async Task<Company?> GetByIdTrackingAsync(int id, CancellationToken ct = default)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public async Task<List<Company>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Companies
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Id)
                .ToListAsync(ct);
        }

        public async Task<bool> CodeExistsAsync(string code, int? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Companies
                .Where(c => c.CommercialName == code && !c.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Company company, CancellationToken ct = default)
        {
            await _context.Companies.AddAsync(company, ct);
        }

        public void Update(Company company)
        {
            _context.Companies.Update(company);
        }

        public async Task SoftDeleteAsync(int id, Guid deletedByUserId, CancellationToken ct = default)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);

            if (company is null) return;

            company.IsDeleted = true;
            company.DeletedByUserId = deletedByUserId;
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedByUserId = deletedByUserId;

            _context.Companies.Update(company);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            return await _context.Companies.AnyAsync(c => c.Id == id && !c.IsDeleted, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}