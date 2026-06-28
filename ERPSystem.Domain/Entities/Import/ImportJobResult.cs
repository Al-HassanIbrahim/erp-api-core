using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Domain.Entities.Import;

/// <summary>
/// Represents the outcome of processing a single row within an <see cref="ImportJob"/>.
/// One record is created per parsed data row regardless of success or failure,
/// providing a full per-row audit trail.
/// </summary>
/// <remarks>
/// Inherits <see cref="BaseEntity"/> (Id, CreatedAt, UpdatedAt, IsDeleted).
/// </remarks>
public class ImportJobResult : BaseEntity
{
    /// <summary>Gets or sets the foreign key to the parent <see cref="ImportJob"/>.</summary>
    public int ImportJobId { get; set; }

    /// <summary>
    /// Gets or sets the 1-based row index in the uploaded source file
    /// (row 1 = first data row after the header).
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this row was validated and persisted
    /// successfully within its per-row transaction.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the validation or processing error message for failed rows.
    /// <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the JSON-serialized original row data captured at parse time via
    /// <c>System.Text.Json.JsonSerializer</c>. Used for audit and retry purposes.
    /// </summary>
    public string? RawData { get; set; }

    /// <summary>Gets or sets the navigation property to the parent import job.</summary>
    public ImportJob ImportJob { get; set; } = null!;
}
