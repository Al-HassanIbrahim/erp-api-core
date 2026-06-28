namespace ERPSystem.Application.Interfaces;

// ─── IFileParser<TRow> ────────────────────────────────────────────────────────

/// <summary>
/// Defines the contract for streaming a raw uploaded file into a list of
/// strongly-typed row model instances. Supports CSV (.csv) and Excel (.xlsx).
/// </summary>
/// <typeparam name="TRow">The strongly-typed row model produced by parsing.</typeparam>
public interface IFileParser<TRow>
{
    /// <summary>
    /// Parses the file stream into an ordered list of row models.
    /// The header row is consumed and excluded from the result.
    /// Column matching is case-insensitive based on header names.
    /// Empty files (header only) return an empty list.
    /// </summary>
    /// <param name="fileStream">A readable stream of the file content. Not seekable required.</param>
    /// <param name="fileName">Original file name including extension; used to detect format.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="Exceptions.BusinessException">
    /// Thrown with <c>IMPORT_FILE_PARSE_FAILED</c> for unsupported extensions or structural errors.
    /// </exception>
    Task<List<TRow>> ParseAsync(Stream fileStream, string fileName, CancellationToken ct = default);
}

// ─── IImportRowValidator<TRow> ────────────────────────────────────────────────

/// <summary>
/// Defines the contract for validating a single parsed import row before it reaches
/// the processor layer. Multiple validators may be registered per <typeparamref name="TRow"/>.
/// </summary>
/// <typeparam name="TRow">The strongly-typed row model to validate.</typeparam>
/// <remarks>
/// <para>
/// Validators MUST NOT persist data. Side-effect-free DB reads for uniqueness or
/// existence checks are permitted. Implement a request-scoped cache (lazy Dictionary)
/// to avoid N+1 queries across rows.
/// </para>
/// <para>
/// The <c>ImportService</c> resolves all registered validators via
/// <c>IEnumerable&lt;IImportRowValidator&lt;TRow&gt;&gt;</c> and applies a fail-fast strategy:
/// processing stops at the first failing validator for each row.
/// </para>
/// </remarks>
public interface IImportRowValidator<TRow>
{
    /// <summary>
    /// Validates a single parsed row in the context of the specified tenant company.
    /// </summary>
    /// <param name="row">The parsed row model. Never null.</param>
    /// <param name="rowNumber">1-based row index; include in error messages for user guidance.</param>
    /// <param name="companyId">Tenant company identifier for data isolation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>(true, null)</c> if valid; <c>(false, errorMessage)</c> on the first violated rule.
    /// </returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(
        TRow row, int rowNumber, int companyId, CancellationToken ct = default);
}

// ─── IImportRowProcessor<TRow> ────────────────────────────────────────────────

/// <summary>
/// Defines the contract for staging a single <em>validated</em> import row for persistence.
/// </summary>
/// <typeparam name="TRow">The strongly-typed row model to process.</typeparam>
/// <remarks>
/// <para>
/// Processors MUST NOT call <c>SaveChangesAsync</c>. Transaction management (begin /
/// commit / rollback) is owned exclusively by <c>ImportService.RunImportPipelineAsync</c>
/// via <c>IUnitOfWork.ExecuteInTransactionAsync</c>. This eliminates phantom records:
/// if processing throws after partial writes, the entire row is rolled back.
/// </para>
/// <para>
/// Processors that perform DB lookups (e.g., resolving FKs by name) should use a
/// request-scoped cache (lazy Dictionary field) to avoid N+1 queries.
/// </para>
/// <para>Business-rule violations must throw <c>BusinessException</c>.</para>
/// </remarks>
public interface IImportRowProcessor<TRow>
{
    /// <summary>
    /// Stages the validated row for insertion by adding it to the EF change tracker.
    /// Does NOT call <c>SaveChangesAsync</c> — the caller owns the transaction.
    /// </summary>
    /// <param name="row">The validated row model. Never null.</param>
    /// <param name="companyId">Tenant company identifier for scoping created entities.</param>
    /// <param name="actorUserId">User identity for <c>CreatedByUserId</c> audit fields.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ProcessAsync(TRow row, int companyId, Guid actorUserId, CancellationToken ct = default);
}

// ─── IPostCommitNotifiable ────────────────────────────────────────────────────

/// <summary>
/// Opt-in interface for import row processors that need to react immediately after
/// a per-row transaction commits successfully.
/// </summary>
/// <remarks>
/// <para>
/// The canonical use case is <c>CategoryImportRowProcessor</c>: EF Core only assigns
/// the database PK after the INSERT executes inside <c>SaveChangesAsync</c>. By
/// implementing this interface, the processor can cache the newly assigned PK so
/// subsequent rows can reference the just-imported entity as a parent without an
/// extra DB round-trip.
/// </para>
/// <para>
/// <c>ImportService</c> calls <see cref="NotifyCommitted"/> on every registered
/// <see cref="IPostCommitNotifiable"/> processor after each successful
/// <c>IUnitOfWork.ExecuteInTransactionAsync</c> completes. It never references
/// concrete processor types — any processor implementing this interface is
/// automatically notified.
/// </para>
/// </remarks>
public interface IPostCommitNotifiable
{
    /// <summary>
    /// Called by <c>ImportService</c> immediately after a per-row transaction commits.
    /// Implementations should update internal caches or state that depends on
    /// database-assigned values (e.g., auto-generated PKs).
    /// </summary>
    void NotifyCommitted();
}

// ─── IImportTemplateService ───────────────────────────────────────────────────

/// <summary>
/// Generates downloadable CSV import templates for all supported entity types.
/// </summary>
public interface IImportTemplateService
{
    /// <summary>
    /// Returns the ordered header names for the given entity type.
    /// These match the column names expected in import files exactly.
    /// </summary>
    /// <exception cref="Exceptions.BusinessException">
    /// Thrown with <c>IMPORT_UNKNOWN_ENTITY_TYPE</c> for unrecognised entity types.
    /// </exception>
    string[] GetHeaders(string entityType);

    /// <summary>
    /// Builds a UTF-8 BOM CSV file containing only the header row.
    /// Ready to download as <c>Content-Type: text/csv</c>.
    /// </summary>
    Task<byte[]> BuildTemplateCsvAsync(string entityType, CancellationToken ct = default);
}

// ─── IImportFileStore ─────────────────────────────────────────────────────────

/// <summary>
/// Abstraction for persisting uploaded import file bytes so they survive the HTTP
/// request lifecycle and can be retrieved by the background worker.
/// </summary>
/// <remarks>
/// The default infrastructure implementation writes to local disk. For cloud
/// deployments, swap with an Azure Blob Storage or S3 adapter.
/// </remarks>
public interface IImportFileStore
{
    /// <summary>
    /// Writes file bytes to durable storage under <c>{companyId}/{storageKey}</c>.
    /// </summary>
    Task SaveAsync(int companyId, string storageKey, byte[] content, CancellationToken ct = default);

    /// <summary>
    /// Reads previously saved file bytes. Returns <c>null</c> if the file no longer exists.
    /// </summary>
    Task<byte[]?> LoadAsync(int companyId, string storageKey, CancellationToken ct = default);

    /// <summary>
    /// Deletes a stored file. Non-throwing: if the file is already gone, this is a no-op.
    /// </summary>
    Task DeleteAsync(int companyId, string storageKey, CancellationToken ct = default);
}

// ─── IImportBackgroundJobService ─────────────────────────────────────────────

/// <summary>
/// Abstraction for enqueuing the import pipeline execution as a background job.
/// The default implementation uses Hangfire. Swap for Azure Service Bus, etc. without
/// touching application-layer code.
/// </summary>
public interface IImportBackgroundJobService
{
    /// <summary>
    /// Enqueues the import pipeline for <paramref name="importJobId"/>.
    /// Returns immediately; processing happens asynchronously in a background worker.
    /// </summary>
    void Enqueue(int importJobId);
}