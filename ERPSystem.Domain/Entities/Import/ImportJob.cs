using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.Import;

/// <summary>
/// Represents a bulk import job. Tracks lifecycle, progress counters, and per-row results
/// for a single file import operation for any supported entity type.
/// </summary>
/// <remarks>
/// Inherits <see cref="AuditableEntity"/> (Id, CreatedAt, UpdatedAt, IsDeleted,
/// CreatedByUserId, UpdatedByUserId, DeletedByUserId) and is company-scoped via
/// <see cref="ICompanyEntity"/>.
/// </remarks>
public class ImportJob : AuditableEntity, ICompanyEntity
{
    /// <summary>Gets or sets the tenant company identifier. All queries must filter by this.</summary>
    public int CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the entity type targeted by this job.
    /// Valid values: "Product", "Category", "UnitOfMeasure". Stored in original client casing.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalised (uppercased) entity type — a persisted computed column
    /// used for index-friendly case-insensitive filtering without calling SQL UPPER() inline.
    /// </summary>
    public string EntityTypeNormalised { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sanitized original file name (path components stripped at ingestion).
    /// Used for audit and display purposes only.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key used to retrieve the uploaded file bytes from
    /// <c>IImportFileStore</c>. Generated at enqueue time; cleared after processing.
    /// </summary>
    public string? FileStorageKey { get; set; }

    /// <summary>
    /// Gets or sets the optional client-supplied idempotency key.
    /// A unique filtered index (per company, non-null, non-deleted) prevents duplicate jobs.
    /// </summary>
    public string? IdempotencyKey { get; set; }

    /// <summary>Gets or sets the total number of data rows parsed from the file.</summary>
    public int TotalRows { get; set; }

    /// <summary>Gets or sets the count of rows successfully validated and persisted.</summary>
    public int SuccessCount { get; set; }

    /// <summary>Gets or sets the count of rows that failed validation or processing.</summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the current lifecycle status.
    /// Transitions: Pending → Processing → Completed | CompletedWithErrors | Failed | Cancelled.
    /// </summary>
    public ImportJobStatus Status { get; set; }

    /// <summary>Gets or sets the UTC timestamp when row processing started.</summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp when processing finished or was cancelled.</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Gets or sets a top-level error description for job-level failures.
    /// Null when the job processed rows normally (even if individual rows failed).
    /// </summary>
    public string? ErrorSummary { get; set; }

    /// <summary>
    /// Gets or sets the per-row import results.
    /// Populated incrementally during processing; cascade-deleted with the job.
    /// </summary>
    public ICollection<ImportJobResult> Results { get; set; } = new List<ImportJobResult>();
}
