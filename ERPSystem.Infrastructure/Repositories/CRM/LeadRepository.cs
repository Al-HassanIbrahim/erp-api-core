using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.CRM;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.CRM;

public sealed class LeadRepository : ILeadRepository
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _current;

    private int CurrentCompanyId => _current.CompanyId;

    public LeadRepository(AppDbContext db, ICurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    private void Ensure(int companyId) => TenantGuard.EnsureCompany(companyId, CurrentCompanyId);

    private IQueryable<Lead> ScopedLeads(int companyId)
    {
        Ensure(companyId);
        return _db.Leads.Where(x => x.CompanyId == companyId);
    }

    public Task<Lead?> GetByIdAsync(int id, int companyId, CancellationToken ct = default)
        => ScopedLeads(companyId).FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Lead>> ListAsync(
        int companyId,
        LeadStatus? stage = null,
        LeadSource? source = null,
        Guid? assignedToId = null,
        string? search = null,
        CancellationToken ct = default)
    {
        IQueryable<Lead> q = ScopedLeads(companyId).AsNoTracking();

        if (stage.HasValue)
            q = q.Where(x => x.Stage == stage.Value);

        if (source.HasValue)
            q = q.Where(x => x.Source == source.Value);

        if (assignedToId.HasValue)
            q = q.Where(x => x.AssignedToId == assignedToId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(x =>
                x.Name.Contains(s) ||
                x.CompanyName.Contains(s) ||
                (x.Email != null && x.Email.Contains(s)) ||
                (x.PhoneNumber != null && x.PhoneNumber.Contains(s)));
        }

        return await q
            .OrderByDescending(x => x.LastContactDate ?? x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Lead lead, int companyId, CancellationToken ct = default)
    {
        Ensure(companyId);

        // enforce tenant on insert
        lead.CompanyId = companyId;

        await _db.Leads.AddAsync(lead, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task UpdateAsync(Lead lead, int companyId)
    {
        Ensure(companyId);

        if (lead.CompanyId != companyId)
            throw new UnauthorizedAccessException("Cross-company update is not allowed.");

        _db.Leads.Update(lead);
        _db.SaveChanges();
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, int companyId, CancellationToken ct = default)
    {
        Ensure(companyId);

        var entity = await _db.Leads.FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId, ct);
        if (entity == null) return;

        _db.Leads.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

}
