using ERPSystem.Domain.Entities.Import;

namespace ERPSystem.Domain.Abstractions;

/// <summary>
/// Repository contract for <see cref="ImportJob"/> persistence operations.
/// All tenant-facing queries are company-scoped and exclude soft-deleted records.
/// </summary>
public interface IImportJobRepository
{
    /// <summary>
    /// Retrieves a job by ID scoped to the given company. Returns <c>null</c> if not found,
    /// soft-deleted, or belonging to a different company. Does not include row results.
    /// </summary>
    Task<ImportJob?> GetByIdAsync(int id, int companyId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a job by ID without company scoping, for use exclusively by the background
    /// worker which resolves company context from the persisted job record itself.
    /// Change-tracking is enabled so the worker can mutate and save the entity directly.
    /// </summary>
    Task<ImportJob?> GetByIdInternalAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a job with all per-row results eagerly loaded, scoped to the given company.
    /// Returns <c>null</c> if not found, soft-deleted, or belonging to a different company.
    /// </summary>
    Task<ImportJob?> GetByIdWithResultsAsync(int id, int companyId, CancellationToken ct = default);

    /// <summary>
    /// Returns all non-deleted jobs for the given company ordered by creation date descending.
    /// Optionally filtered by entity type (case-insensitive).
    /// </summary>
    Task<List<ImportJob>> GetAllByCompanyAsync(int companyId, string? entityType, CancellationToken ct = default);

    /// <summary>
    /// Looks up an existing job by its idempotency key within the given company.
    /// Returns <c>null</c> if no matching job exists. Used to prevent duplicate submissions.
    /// </summary>
    Task<ImportJob?> GetByIdempotencyKeyAsync(string idempotencyKey, int companyId, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if the job's status is <c>Cancelling</c>, indicating the
    /// background worker should stop at the next inter-row checkpoint.
    /// </summary>
    Task<bool> IsCancellationRequestedAsync(int importJobId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new <see cref="ImportJob"/> to the EF change tracker for insertion.
    /// Call <see cref="SaveChangesAsync"/> to commit.
    /// </summary>
    Task AddAsync(ImportJob job, CancellationToken ct = default);

    /// <summary>
    /// Atomically marks a job as <c>Failed</c> with the given error summary via a direct
    /// UPDATE — safe to call even when the entity is tracked elsewhere (e.g., in the worker).
    /// </summary>
    Task MarkJobFailedAsync(int importJobId, string errorSummary, CancellationToken ct = default);

    /// <summary>
    /// Atomically transitions a job to <c>Cancelling</c> if it is currently <c>Pending</c>
    /// or <c>Processing</c>. Returns <c>true</c> if the update succeeded; <c>false</c> if
    /// the job is in a terminal state or does not belong to the given company.
    /// </summary>
    Task<bool> RequestCancellationAsync(int importJobId, int companyId, CancellationToken ct = default);

    /// <summary>
    /// Returns a page of <see cref="ImportJobResult"/> rows for the given job, scoped to the
    /// given company. Ordered ascending by row number.
    /// </summary>
    /// <param name="id">Import job primary key.</param>
    /// <param name="companyId">Tenant company ID for isolation.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Number of rows per page (clamped to 1–1000 in the implementation).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A tuple containing the page of results and the total number of result rows for the job.
    /// Returns <c>null</c> if the job is not found or belongs to a different company.
    /// </returns>
    Task<(ImportJob Job, IReadOnlyList<ImportJobResult> Results, int TotalCount)?> GetPagedResultsAsync(
        int id, int companyId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Commits all pending EF change-tracked operations to the database.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Clears all EF-tracked entities from the change tracker and re-attaches
    /// <paramref name="job"/> so that header-level mutations (Status, SuccessCount, etc.)
    /// remain tracked for the final <see cref="SaveChangesAsync"/> at pipeline completion.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Called by <c>ImportService</c> every 500 rows after flushing accumulated
    /// <c>ImportJobResult</c> rows to the database. Without this call the EF change tracker
    /// retains a strong reference to every tracked entity indefinitely, causing unbounded
    /// heap growth for large imports (100K rows × ~500-byte RawData ≈ 50 MB+).
    /// </para>
    /// <para>
    /// This method lives in the repository (Infrastructure layer) rather than in
    /// <c>ImportService</c> (Application layer) to keep EF change-tracker APIs out of
    /// the Application layer, consistent with the architecture constraint that
    /// <c>AppDbContext</c> must not be referenced outside Infrastructure.
    /// </para>
    /// </remarks>
    void DetachAndReattach(ImportJob job);
}