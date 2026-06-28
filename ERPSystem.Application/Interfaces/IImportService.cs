using ERPSystem.Application.DTOs.Import;

namespace ERPSystem.Application.Interfaces;

/// <summary>
/// Top-level service contract for the Bulk Import subsystem.
/// </summary>
/// <remarks>
/// The import lifecycle is split into two phases:
/// <list type="number">
///   <item>
///     <b>Enqueue</b> (<see cref="EnqueueImportAsync"/>): called on the HTTP thread.
///     Validates the request surface, persists a <c>Pending</c> job, stores file bytes,
///     enqueues the background worker, and returns the <c>jobId</c> immediately.
///   </item>
///   <item>
///     <b>Execute</b> (<see cref="ExecuteImportJobAsync"/>): called by the background worker.
///     Runs parse → validate → per-row-transaction process. Never called by the HTTP layer.
///   </item>
/// </list>
/// </remarks>
public interface IImportService
{
    /// <summary>
    /// Validates the request, persists a <c>Pending</c> job, stores file bytes, and
    /// enqueues the background worker. Returns the new job ID immediately.
    /// </summary>
    /// <exception cref="ERPSystem.Application.Exceptions.BusinessException">
    /// <c>IMPORT_UNKNOWN_ENTITY_TYPE</c> | <c>IMPORT_NO_FILE</c> |
    /// <c>IMPORT_FILE_TOO_LARGE</c> | <c>IMPORT_DUPLICATE_REQUEST</c>
    /// </exception>
    Task<int> EnqueueImportAsync(ImportRequest request, CancellationToken ct = default);

    /// <summary>
    /// Executes the full import pipeline for the given job ID.
    /// Called exclusively by the background worker — never by the HTTP layer.
    /// </summary>
    Task ExecuteImportJobAsync(int importJobId, CancellationToken ct = default);

    /// <summary>
    /// Requests cancellation of a <c>Pending</c> or <c>Processing</c> job.
    /// The background worker stops at the next inter-row checkpoint.
    /// </summary>
    /// <exception cref="ERPSystem.Application.Exceptions.BusinessException">
    /// <c>IMPORT_JOB_NOT_FOUND</c> | <c>IMPORT_JOB_NOT_CANCELLABLE</c>
    /// </exception>
    Task CancelJobAsync(int jobId, CancellationToken ct = default);

    /// <summary>Returns the summary of a job scoped to the current tenant, or <c>null</c>.</summary>
    Task<ImportJobDto?> GetJobAsync(int jobId, CancellationToken ct = default);

    /// <summary>
    /// Returns a single page of per-row results for the given job, or <c>null</c> if not found.
    /// </summary>
    /// <param name="jobId">Import job primary key.</param>
    /// <param name="page">1-based page number (defaults to 1).</param>
    /// <param name="pageSize">
    /// Results per page. Clamped server-side to [1, 1000]. Recommended default: 500.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    Task<ImportJobPagedResultDto?> GetJobResultsAsync(
        int jobId, int page = 1, int pageSize = 500, CancellationToken ct = default);

    /// <summary>
    /// Returns all jobs for the current tenant ordered by creation date descending.
    /// Optionally filtered by entity type (case-insensitive).
    /// </summary>
    Task<IReadOnlyList<ImportJobDto>> GetAllJobsAsync(string? entityType, CancellationToken ct = default);

    /// <summary>Returns the CSV template byte array for the given entity type.</summary>
    /// <exception cref="ERPSystem.Application.Exceptions.BusinessException">
    /// <c>IMPORT_UNKNOWN_ENTITY_TYPE</c>
    /// </exception>
    Task<byte[]> DownloadTemplateAsync(string entityType, CancellationToken ct = default);
}
