using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Import;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Import;

/// <summary>
/// EF Core implementation of <see cref="IImportJobRepository"/>.
/// Tenant-facing queries are company-scoped and exclude soft-deleted records.
/// </summary>
public sealed class ImportJobRepository : IImportJobRepository
{
    private readonly AppDbContext _db;

    /// <summary>Initializes a new instance of <see cref="ImportJobRepository"/>.</summary>
    public ImportJobRepository(AppDbContext db) => _db = db;

    // ─── Single-record reads ──────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ImportJob?> GetByIdAsync(int id, int companyId, CancellationToken ct = default) =>
        await _db.ImportJobs
            .AsNoTracking()
            .Where(j => j.Id == id && j.CompanyId == companyId && !j.IsDeleted)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<ImportJob?> GetByIdInternalAsync(int id, CancellationToken ct = default) =>
        // Change-tracking ON — background worker mutates and saves this entity directly.
        // No company scope — worker resolves company from the job record itself.
        await _db.ImportJobs
            .Where(j => j.Id == id && !j.IsDeleted)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<ImportJob?> GetByIdWithResultsAsync(
        int id, int companyId, CancellationToken ct = default) =>
        await _db.ImportJobs
            .Include(j => j.Results.Where(r => !r.IsDeleted))
            .Where(j => j.Id == id && j.CompanyId == companyId && !j.IsDeleted)
            .FirstOrDefaultAsync(ct);

    // ─── List read ────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<List<ImportJob>> GetAllByCompanyAsync(
        int companyId, string? entityType, CancellationToken ct = default)
    {
        var query = _db.ImportJobs
            .AsNoTracking()
            .Where(j => j.CompanyId == companyId && !j.IsDeleted);

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            // Filter on the persisted computed column (UPPER([EntityType])) for
            // index-friendly case-insensitive comparison without calling SQL UPPER() inline.
            string normalised = entityType.Trim().ToUpperInvariant();
            query = query.Where(j => j.EntityTypeNormalised == normalised);
        }

        return await query.OrderByDescending(j => j.CreatedAt).ToListAsync(ct);
    }

    // ─── Idempotency ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<ImportJob?> GetByIdempotencyKeyAsync(
        string idempotencyKey, int companyId, CancellationToken ct = default) =>
        await _db.ImportJobs
            .AsNoTracking()
            .Where(j => j.IdempotencyKey == idempotencyKey
                     && j.CompanyId == companyId
                     && !j.IsDeleted)
            .FirstOrDefaultAsync(ct);

    // ─── Cancellation ─────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<bool> IsCancellationRequestedAsync(
        int importJobId, CancellationToken ct = default) =>
        await _db.ImportJobs
            .AsNoTracking()
            .Where(j => j.Id == importJobId)
            .Select(j => j.Status == ImportJobStatus.Cancelling)
            .FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<bool> RequestCancellationAsync(
        int importJobId, int companyId, CancellationToken ct = default)
    {
        // Atomic conditional UPDATE — only cancellable statuses, correct company.
        // ExecuteUpdateAsync skips the change tracker entirely, preventing stale-entity conflicts.
        int affected = await _db.ImportJobs
            .Where(j => j.Id == importJobId
                     && j.CompanyId == companyId
                     && !j.IsDeleted
                     && (j.Status == ImportJobStatus.Pending
                      || j.Status == ImportJobStatus.Processing))
            .ExecuteUpdateAsync(
                s => s.SetProperty(j => j.Status, ImportJobStatus.Cancelling), ct);

        return affected > 0;
    }

    // ─── Failure recording ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task MarkJobFailedAsync(
        int importJobId, string errorSummary, CancellationToken ct = default) =>
        await _db.ImportJobs
            .Where(j => j.Id == importJobId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(j => j.Status, ImportJobStatus.Failed)
                .SetProperty(j => j.ErrorSummary, errorSummary)
                .SetProperty(j => j.CompletedAt, DateTime.UtcNow), ct);

    // ─── Paginated results ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<(ImportJob Job, IReadOnlyList<ImportJobResult> Results, int TotalCount)?> GetPagedResultsAsync(
        int id, int companyId, int page, int pageSize, CancellationToken ct = default)
    {
        // Clamp page size to a safe band — callers cannot bypass this at the DB level.
        pageSize = Math.Clamp(pageSize, 1, 1000);
        int skip = (Math.Max(page, 1) - 1) * pageSize;

        // Load the job header (no results navigation) — fast, tiny row.
        var job = await _db.ImportJobs
            .AsNoTracking()
            .Where(j => j.Id == id && j.CompanyId == companyId && !j.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (job is null)
            return null;

        // Count and page results in separate efficient queries.
        // Separating count from data avoids a CROSS APPLY / sub-select on large tables.
        var baseQuery = _db.ImportJobResults
            .AsNoTracking()
            .Where(r => r.ImportJobId == id && !r.IsDeleted)
            .OrderBy(r => r.RowNumber);

        int totalCount = await baseQuery.CountAsync(ct);

        var results = await baseQuery
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(ct);

        return (job, results.AsReadOnly(), totalCount);
    }

    // ─── Write ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task AddAsync(ImportJob job, CancellationToken ct = default) =>
        await _db.ImportJobs.AddAsync(job, ct);

    /// <inheritdoc/>
    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);

    // ─── Change-tracker management ────────────────────────────────────────────

    /// <inheritdoc/>
    public void DetachAndReattach(ImportJob job)
    {
        // Step 1: Detach every tracked entity — releases all ImportJobResult references
        //         so the GC can collect the objects accumulated during the last 500-row batch.
        //         Without this, EF holds a strong reference to each entity indefinitely
        //         even after job.Results.Clear() removes them from the in-memory list.
        _db.ChangeTracker.Clear();

        // Step 2: Re-attach the job entity in Unchanged state so that subsequent
        //         mutations to job.Status, job.SuccessCount, job.FailureCount etc.
        //         are still tracked and will be flushed correctly in the final
        //         SaveChangesAsync call at pipeline completion.
        _db.Attach(job);
    }
}