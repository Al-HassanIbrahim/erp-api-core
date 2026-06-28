namespace ERPSystem.Domain.Enums;

/// <summary>
/// Represents the lifecycle state of a bulk <see cref="ERPSystem.Domain.Entities.Import.ImportJob"/>.
/// </summary>
public enum ImportJobStatus
{
    /// <summary>
    /// Job record created, file stored, background worker enqueued but not yet started.
    /// </summary>
    Pending = 0,

    /// <summary>The import pipeline is actively parsing, validating, and processing rows.</summary>
    Processing = 1,

    /// <summary>All rows succeeded. <c>FailureCount == 0</c>.</summary>
    Completed = 2,

    /// <summary>
    /// Processing finished but at least one row failed.
    /// <c>SuccessCount &gt; 0 &amp;&amp; FailureCount &gt; 0</c>.
    /// </summary>
    CompletedWithErrors = 3,

    /// <summary>
    /// Job failed before any rows could be processed (e.g., file parse error).
    /// <c>SuccessCount == 0</c>.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// A cancellation request was received. The background worker will stop at the
    /// next inter-row checkpoint and transition to <see cref="Cancelled"/>.
    /// </summary>
    Cancelling = 5,

    /// <summary>
    /// The import was stopped by user request. Rows already committed remain in the database.
    /// </summary>
    Cancelled = 6
}
