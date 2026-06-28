using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.DTOs.Import;

// ─── Inbound DTO ─────────────────────────────────────────────────────────────

/// <summary>
/// Represents a bulk import request constructed by the API layer.
/// </summary>
/// <remarks>
/// The API layer reads the multipart stream into <see cref="FileBytes"/> and disposes
/// the stream before constructing this object. A raw byte array — not an open stream —
/// crosses the service boundary, making it safe to hand off to a background worker.
/// </remarks>
public sealed class ImportRequest
{
    /// <summary>Entity type: "Product", "Category", or "UnitOfMeasure" (case-insensitive).</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Raw file bytes. Read from the multipart stream by the controller.</summary>
    public byte[] FileBytes { get; set; } = [];

    /// <summary>
    /// Original file name including extension (e.g., <c>products.csv</c>).
    /// Used by the parser to detect format; sanitized before storage.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Optional client-generated idempotency key (UUID recommended).
    /// A second submission with the same key returns the existing job ID instead of
    /// creating a duplicate. Pass <c>null</c> to skip idempotency checking.
    /// </summary>
    public string? IdempotencyKey { get; set; }
}

// ─── Outbound DTOs ────────────────────────────────────────────────────────────

/// <summary>
/// Summary view of a bulk import job returned on list and creation endpoints.
/// </summary>
public class ImportJobDto
{
    /// <summary>Import job primary key.</summary>
    public int Id { get; set; }

    /// <summary>Entity type that was imported (e.g., "Product").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Original uploaded file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Total number of data rows parsed from the file.</summary>
    public int TotalRows { get; set; }

    /// <summary>Count of rows successfully imported.</summary>
    public int SuccessCount { get; set; }

    /// <summary>Count of rows that failed validation or processing.</summary>
    public int FailureCount { get; set; }

    /// <summary>Current lifecycle status of the job.</summary>
    public ImportJobStatus Status { get; set; }

    /// <summary>UTC timestamp when row processing started. <c>null</c> if still pending.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>UTC timestamp when processing completed or was cancelled.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Top-level failure reason for job-level failures (parse errors, unknown entity type).
    /// <c>null</c> for jobs that reached the row-processing phase.
    /// </summary>
    public string? ErrorSummary { get; set; }

    /// <summary>UTC timestamp when the job record was created.</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed view of a bulk import job including all per-row results.
/// Returned by <c>GET /api/import/{jobId}/results</c>.
/// </summary>
public sealed class ImportJobDetailDto : ImportJobDto
{
    /// <summary>
    /// Ordered list of per-row import results, sorted ascending by
    /// <see cref="ImportJobResultDto.RowNumber"/>.
    /// </summary>
    public List<ImportJobResultDto> Results { get; set; } = [];
}

/// <summary>Represents the outcome of processing a single row within an import job.</summary>
public sealed class ImportJobResultDto
{
    /// <summary>Row result record primary key.</summary>
    public int Id { get; set; }

    /// <summary>1-based row index in the source file.</summary>
    public int RowNumber { get; set; }

    /// <summary>Whether this row was processed successfully.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Error message for failed rows. <c>null</c> on success.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// JSON-serialized original row data captured at parse time.
    /// Allows clients to review exact values without re-uploading.
    /// </summary>
    public string? RawData { get; set; }
}

/// <summary>
/// Paged view of per-row results for a bulk import job.
/// Returned by <c>GET /api/import/{jobId}/results?page=1&amp;pageSize=500</c>.
/// </summary>
public sealed class ImportJobPagedResultDto
{
    // ── Job header ──────────────────────────────────────────────────────────

    /// <summary>Import job primary key.</summary>
    public int Id { get; set; }

    /// <summary>Entity type that was imported (e.g., "Product").</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Original uploaded file name.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Total number of data rows parsed from the file.</summary>
    public int TotalRows { get; set; }

    /// <summary>Count of rows successfully imported.</summary>
    public int SuccessCount { get; set; }

    /// <summary>Count of rows that failed validation or processing.</summary>
    public int FailureCount { get; set; }

    /// <summary>Current lifecycle status of the job.</summary>
    public ERPSystem.Domain.Enums.ImportJobStatus Status { get; set; }

    /// <summary>UTC timestamp when row processing started. <c>null</c> if still pending.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>UTC timestamp when processing completed or was cancelled.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>Top-level failure reason for job-level failures. <c>null</c> otherwise.</summary>
    public string? ErrorSummary { get; set; }

    /// <summary>UTC timestamp when the job record was created.</summary>
    public DateTime CreatedAt { get; set; }

    // ── Pagination metadata ─────────────────────────────────────────────────

    /// <summary>Current 1-based page number.</summary>
    public int Page { get; set; }

    /// <summary>Number of results per page as used in this response.</summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of <see cref="ImportJobResult"/> rows stored for this job.
    /// Use <c>Math.Ceiling(TotalResultCount / (double)PageSize)</c> to determine total pages.
    /// </summary>
    public int TotalResultCount { get; set; }

    // ── Current page of results ─────────────────────────────────────────────

    /// <summary>
    /// The result rows for the current page, ordered ascending by
    /// <see cref="ImportJobResultDto.RowNumber"/>.
    /// </summary>
    public List<ImportJobResultDto> Results { get; set; } = [];
}