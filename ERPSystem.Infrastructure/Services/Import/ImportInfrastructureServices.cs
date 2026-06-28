using ERPSystem.Application.Interfaces;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ERPSystem.Infrastructure.Services.Import;

// ═══════════════════════════════════════════════════════════════════════════════
// LOCAL DISK FILE STORE
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Configuration options for <see cref="LocalDiskImportFileStore"/>.
/// Bind from appsettings.json: <c>"ImportFileStore": { "BasePath": "..." }</c>
/// </summary>
public sealed class ImportFileStoreOptions
{
    /// <summary>
    /// Base directory for stored import files.
    /// Defaults to a subdirectory of the system temp folder.
    /// </summary>
    public string BasePath { get; set; } =
        Path.Combine(Path.GetTempPath(), "erp-import-files");
}

/// <summary>
/// <see cref="IImportFileStore"/> implementation that writes import file bytes to local disk.
/// Files are stored at <c>{BasePath}/{companyId}/{storageKey}.bin</c>.
/// </summary>
/// <remarks>
/// Replace with an Azure Blob Storage or AWS S3 adapter for cloud deployments —
/// only this class changes; no application-layer code is touched.
/// </remarks>
public sealed class LocalDiskImportFileStore : IImportFileStore
{
    private readonly ImportFileStoreOptions          _options;
    private readonly ILogger<LocalDiskImportFileStore> _logger;

    /// <summary>Initializes a new instance of <see cref="LocalDiskImportFileStore"/>.</summary>
    public LocalDiskImportFileStore(
        IOptions<ImportFileStoreOptions> options,
        ILogger<LocalDiskImportFileStore> logger)
    {
        _options = options.Value;
        _logger  = logger;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(
        int companyId, string storageKey, byte[] content, CancellationToken ct = default)
    {
        string path = BuildPath(companyId, storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, content, ct);

        _logger.LogDebug(
            "Import file stored. CompanyId={C} Key={K} Bytes={B}",
            companyId, storageKey, content.Length);
    }

    /// <inheritdoc/>
    public async Task<byte[]?> LoadAsync(
        int companyId, string storageKey, CancellationToken ct = default)
    {
        string path = BuildPath(companyId, storageKey);
        if (!File.Exists(path))
        {
            _logger.LogWarning(
                "Import file not found. CompanyId={C} Key={K} Path={P}",
                companyId, storageKey, path);
            return null;
        }

        return await File.ReadAllBytesAsync(path, ct);
    }

    /// <inheritdoc/>
    public Task DeleteAsync(int companyId, string storageKey, CancellationToken ct = default)
    {
        string path = BuildPath(companyId, storageKey);
        try { if (File.Exists(path)) File.Delete(path); }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Import file delete failed (non-fatal). CompanyId={C} Key={K}", companyId, storageKey);
        }
        return Task.CompletedTask;
    }

    private string BuildPath(int companyId, string storageKey)
    {
        // Sanitize storageKey defensively — it's a Guid string but be safe
        string safeKey = Path.GetFileName(storageKey.Replace("..", string.Empty));
        return Path.Combine(_options.BasePath, companyId.ToString(), $"{safeKey}.bin");
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// HANGFIRE BACKGROUND JOB SERVICE
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// <see cref="IImportBackgroundJobService"/> backed by Hangfire's fire-and-forget queue.
/// </summary>
/// <remarks>
/// <para>
/// Hangfire serializes the job to its backing store (SQL Server, Redis, etc.) and
/// dispatches it to a background worker process. The HTTP thread returns immediately.
/// </para>
/// <para>
/// The target method <see cref="IImportService.ExecuteImportJobAsync"/> is resolved from
/// the DI container by Hangfire's job activator at execution time, so the worker gets
/// its own DI scope and its own <c>DbContext</c> instance — isolated from the HTTP request.
/// </para>
/// <para>
/// Hangfire retries failed jobs (infrastructure failures only; domain failures are caught
/// inside the pipeline and never surface as job exceptions) with exponential back-off.
/// </para>
/// <para>
/// <b>Queue isolation:</b> Jobs are always enqueued to the <c>"import"</c> queue via the
/// explicit <c>queue</c> overload of <see cref="BackgroundJob.Enqueue{T}"/>. This matches
/// the server configuration <c>Queues = ["import", "default"]</c> and means the queue can
/// later be moved to a dedicated worker with <c>Queues = ["import"]</c> only — no jobs will
/// ever be stranded in <c>"default"</c>.
/// </para>
/// </remarks>
public sealed class HangfireImportBackgroundJobService : IImportBackgroundJobService
{
    /// <summary>
    /// Hangfire queue name for all bulk import jobs.
    /// Must match the <c>Queues</c> array in the server's <c>BackgroundJobServerOptions</c>.
    /// </summary>
    public const string QueueName = "import";

    /// <inheritdoc/>
    public void Enqueue(int importJobId) =>
        BackgroundJob.Enqueue<IImportService>(
            QueueName,
            svc => svc.ExecuteImportJobAsync(importJobId, CancellationToken.None));
}
