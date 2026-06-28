using System.Text.Json;
using ERPSystem.Application.DTOs.Import;
using ERPSystem.Application.DTOs.Import.Rows;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Import;
using ERPSystem.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ERPSystem.Application.Services.Import;

/// <summary>
/// Orchestrates the Bulk Import pipeline in two phases:
/// <list type="number">
///   <item><b>Enqueue</b> — validates request, persists Pending job, stores file bytes, enqueues worker. Returns immediately.</item>
///   <item><b>Execute</b> — called by background worker: parse → validate → per-row-transaction → process.</item>
/// </list>
/// <para>
/// <b>Architecture compliance:</b>
/// <list type="bullet">
///   <item>No <c>AppDbContext</c> reference — EF is accessed exclusively via <c>IUnitOfWork</c>
///     (for per-row transactions) and <c>IImportJobRepository</c> (for saves and change-tracker
///     management). This matches the pattern used by every other Application-layer service.</item>
///   <item>No concrete processor types referenced — post-commit notifications are dispatched
///     via <see cref="IPostCommitNotifiable"/> so adding a new processor never requires
///     changing this class.</item>
/// </list>
/// </para>
/// Open/Closed: new entity types need only DI registrations — this class never changes.
/// </summary>
public sealed class ImportService : IImportService
{
    /// <summary>Maximum allowed file size: 50 MB.</summary>
    public const int MaxFileSizeBytes = 50 * 1024 * 1024;

    /// <summary>Maximum allowed rows per import file.</summary>
    public const int MaxRowCount = 100_000;

    private readonly IImportJobRepository _importJobRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IServiceProvider _serviceProvider;
    private readonly IImportTemplateService _templateService;
    private readonly IImportFileStore _fileStore;
    private readonly IImportBackgroundJobService _backgroundJobService;
    private readonly ILogger<ImportService> _logger;

    /// <summary>Initializes a new instance of <see cref="ImportService"/>.</summary>
    public ImportService(
        IImportJobRepository importJobRepo,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IServiceProvider serviceProvider,
        IImportTemplateService templateService,
        IImportFileStore fileStore,
        IImportBackgroundJobService backgroundJobService,
        ILogger<ImportService> logger)
    {
        _importJobRepo = importJobRepo;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _serviceProvider = serviceProvider;
        _templateService = templateService;
        _fileStore = fileStore;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PHASE 1 — ENQUEUE (HTTP request thread)
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<int> EnqueueImportAsync(ImportRequest request, CancellationToken ct = default)
    {
        // ── Validate entity type ────────────────────────────────────────────
        string entityTypeKey = request.EntityType.Trim().ToUpperInvariant();
        ValidateEntityType(entityTypeKey, request.EntityType);

        // ── Validate file bytes ─────────────────────────────────────────────
        if (request.FileBytes is null || request.FileBytes.Length == 0)
            throw BusinessErrors.ImportNoFile();

        if (request.FileBytes.Length > MaxFileSizeBytes)
            throw BusinessErrors.ImportFileTooLarge(MaxFileSizeBytes / (1024 * 1024));

        // ── Idempotency check ───────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existing = await _importJobRepo.GetByIdempotencyKeyAsync(
                request.IdempotencyKey, _currentUser.CompanyId, ct);

            if (existing is not null)
            {
                _logger.LogInformation(
                    "Duplicate import. IdempotencyKey={Key} ExistingJobId={Id}",
                    request.IdempotencyKey, existing.Id);
                throw BusinessErrors.ImportDuplicateRequest(existing.Id);
            }
        }

        // ── Persist job in Pending status ───────────────────────────────────
        string storageKey = Guid.NewGuid().ToString("N");

        var job = new ImportJob
        {
            CompanyId = _currentUser.CompanyId,
            EntityType = request.EntityType.Trim(),
            EntityTypeNormalised = entityTypeKey,
            FileName = SanitizeFileName(request.FileName),
            FileStorageKey = storageKey,
            IdempotencyKey = request.IdempotencyKey?.Trim(),
            TotalRows = 0,
            SuccessCount = 0,
            FailureCount = 0,
            Status = ImportJobStatus.Pending,
            CreatedByUserId = _currentUser.UserId
        };

        await _importJobRepo.AddAsync(job, ct);
        await _importJobRepo.SaveChangesAsync(ct);

        int jobId = job.Id;

        _logger.LogInformation(
            "Import job created. JobId={JobId} EntityType={EntityType} FileName={FileName} CompanyId={CompanyId}",
            jobId, job.EntityType, job.FileName, _currentUser.CompanyId);

        // ── Store file bytes ────────────────────────────────────────────────
        // Done after job persist so we always have a record to mark Failed on error.
        try
        {
            await _fileStore.SaveAsync(_currentUser.CompanyId, storageKey, request.FileBytes, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "File store write failed. JobId={JobId} StorageKey={Key}", jobId, storageKey);

            await _importJobRepo.MarkJobFailedAsync(
                jobId, "Failed to store the uploaded file. Please try again.", ct);

            throw BusinessErrors.ImportFileParseFailed("Unable to store the uploaded file.");
        }

        // ── Enqueue background job ──────────────────────────────────────────
        _backgroundJobService.Enqueue(jobId);

        _logger.LogInformation("Import job enqueued for background processing. JobId={JobId}", jobId);

        return jobId;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PHASE 2 — EXECUTE (Background worker thread)
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task ExecuteImportJobAsync(int importJobId, CancellationToken ct = default)
    {
        var job = await _importJobRepo.GetByIdInternalAsync(importJobId, ct);
        if (job is null)
        {
            _logger.LogError("Background worker: ImportJob {JobId} not found.", importJobId);
            return;
        }

        _logger.LogInformation(
            "Worker starting import. JobId={JobId} EntityType={EntityType}",
            importJobId, job.EntityType);

        job.Status = ImportJobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        await _importJobRepo.SaveChangesAsync(ct);

        try
        {
            await (job.EntityType.Trim().ToUpperInvariant() switch
            {
                "PRODUCT" => RunImportPipelineAsync<ProductImportRow>(job, ct),
                "CATEGORY" => RunImportPipelineAsync<CategoryImportRow>(job, ct),
                "UNITOFMEASURE" => RunImportPipelineAsync<UnitOfMeasureImportRow>(job, ct),
                _ => throw BusinessErrors.ImportUnknownEntityType(job.EntityType)
            });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Unhandled pipeline exception. JobId={JobId}", importJobId);
            await _importJobRepo.MarkJobFailedAsync(
                importJobId, "An unexpected error occurred during import processing.", ct);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CORE PIPELINE (generic, type-safe)
    // ═══════════════════════════════════════════════════════════════════════

    private async Task RunImportPipelineAsync<TRow>(ImportJob job, CancellationToken ct)
        where TRow : class, new()
    {
        // ── Load stored file bytes ──────────────────────────────────────────
        byte[]? fileBytes = await _fileStore.LoadAsync(job.CompanyId, job.FileStorageKey!, ct);
        if (fileBytes is null)
        {
            _logger.LogError(
                "Stored file not found. JobId={JobId} StorageKey={Key}", job.Id, job.FileStorageKey);
            await _importJobRepo.MarkJobFailedAsync(
                job.Id, "The uploaded file could not be retrieved. Please submit a new import.", ct);
            return;
        }

        // ── Parse ───────────────────────────────────────────────────────────
        var parser = _serviceProvider.GetRequiredService<IFileParser<TRow>>();
        List<TRow> rows;

        try
        {
            using var ms = new MemoryStream(fileBytes, writable: false);
            rows = await parser.ParseAsync(ms, job.FileName, ct);
        }
        catch (BusinessException bex)
        {
            await _importJobRepo.MarkJobFailedAsync(job.Id, bex.Message, ct);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Parse failed. JobId={JobId}", job.Id);
            await _importJobRepo.MarkJobFailedAsync(
                job.Id, $"Failed to parse the import file: {ex.Message}", ct);
            return;
        }

        if (rows.Count == 0)
        {
            await _importJobRepo.MarkJobFailedAsync(job.Id,
                "The import file contains no data rows. Ensure the file has at least one row below the header.", ct);
            return;
        }

        if (rows.Count > MaxRowCount)
        {
            await _importJobRepo.MarkJobFailedAsync(job.Id,
                $"The file contains {rows.Count:N0} rows which exceeds the maximum of {MaxRowCount:N0}.", ct);
            return;
        }

        // Update TotalRows now that we know the actual count.
        job.TotalRows = rows.Count;
        await _importJobRepo.SaveChangesAsync(ct);

        // ── Resolve validators and processors once per import ───────────────
        var validators = _serviceProvider.GetServices<IImportRowValidator<TRow>>().ToList();
        var processors = _serviceProvider.GetServices<IImportRowProcessor<TRow>>().ToList();

        // Resolve all processors that opt into post-commit notifications via the interface.
        // No concrete type references — any processor implementing IPostCommitNotifiable
        // is automatically included. Adding a new notifiable processor requires zero changes here.
        var postCommitNotifiables = processors.OfType<IPostCommitNotifiable>().ToList();

        int successCount = 0;
        int failureCount = 0;

        // ── Row processing loop ─────────────────────────────────────────────
        for (int i = 0; i < rows.Count; i++)
        {
            // Host shutdown cancellation (between rows — never mid-row).
            if (ct.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Host shutdown cancellation. JobId={JobId} CompletedRows={N}", job.Id, i);
                break;
            }

            // User-requested cancellation (DB flag, polled between rows).
            if (await _importJobRepo.IsCancellationRequestedAsync(job.Id, ct))
            {
                _logger.LogInformation(
                    "User-requested cancellation. JobId={JobId} CompletedRows={N}", job.Id, i);
                break;
            }

            int rowNumber = i + 1;
            TRow row = rows[i];
            string rawData = JsonSerializer.Serialize(row);

            // ── Validate (fail-fast per row) ────────────────────────────────
            string? validationError = null;
            foreach (var validator in validators)
            {
                var (isValid, errorMessage) = await validator.ValidateAsync(
                    row, rowNumber, job.CompanyId, ct);

                if (!isValid)
                {
                    validationError = errorMessage;
                    break;
                }
            }

            if (validationError is not null)
            {
                job.Results.Add(new ImportJobResult
                {
                    ImportJobId = job.Id,
                    RowNumber = rowNumber,
                    IsSuccess = false,
                    ErrorMessage = validationError,
                    RawData = rawData
                });
                failureCount++;
                continue;
            }

            // ── Process inside a per-row transaction ────────────────────────
            // GUARANTEE: if any processor throws after partial writes, every DB change
            // for this row is rolled back by IUnitOfWork. No phantom records possible.
            //
            // Pattern mirrors PurchaseInvoiceService.PostAsync — IUnitOfWork owns
            // begin / commit / rollback; processors stage entities; repo flushes them.
            // AppDbContext is never referenced here — all EF APIs stay in Infrastructure.
            string? processingError = null;

            try
            {
                await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
                {
                    foreach (var processor in processors)
                        await processor.ProcessAsync(row, job.CompanyId, job.CreatedByUserId, innerCt);

                    // Flush entity writes to the DB inside the open transaction so the DB
                    // assigns PKs before CommitAsync. Processors rely on this for FK resolution.
                    await _importJobRepo.SaveChangesAsync(innerCt);

                }, ct);

                // Post-commit: notify processors that opted into IPostCommitNotifiable.
                // Called only on success — never inside the transaction so cache updates
                // are never rolled back even if a subsequent row fails.
                foreach (var notifiable in postCommitNotifiables)
                    notifiable.NotifyCommitted();
            }
            catch (BusinessException bex)
            {
                // IUnitOfWork has already rolled back the transaction.
                processingError = bex.Message;
                _logger.LogWarning(
                    "Row {Row} business rule violation. JobId={JobId}: {Msg}",
                    rowNumber, job.Id, bex.Message);
            }
            catch (DataConstraintException ex)
            {
                // Already rolled back by UnitOfWork.
                processingError = ex.Message;
                _logger.LogWarning(ex, "Row {Row} DB constraint violation. JobId={JobId}",
                    rowNumber, job.Id);
            }
            catch (Exception ex)
            {
                // IUnitOfWork has already rolled back the transaction.
                processingError = "Unexpected error during row processing.";
                _logger.LogError(ex,
                    "Row {Row} unexpected error. JobId={JobId}", rowNumber, job.Id);
            }

            bool rowSucceeded = processingError is null;
            job.Results.Add(new ImportJobResult
            {
                ImportJobId = job.Id,
                RowNumber = rowNumber,
                IsSuccess = rowSucceeded,
                ErrorMessage = processingError,
                RawData = rawData
            });

            if (rowSucceeded) successCount++;
            else failureCount++;

            // ── Flush + memory release every 500 rows ───────────────────────
            // SaveChangesAsync writes the accumulated ImportJobResult rows to the DB.
            // DetachAndReattach (implemented in Infrastructure) then:
            //   1. Calls dbContext.ChangeTracker.Clear() — releases strong references to
            //      all tracked ImportJobResult instances so the GC can collect them.
            //   2. Calls dbContext.Attach(job) — re-tracks the job entity so header-level
            //      mutations (Status, SuccessCount, etc.) are still flushed at the end.
            //
            // Without this, a 100K-row import accumulates ~50 MB+ of retained objects
            // in the EF change tracker by the final row.
            if (job.Results.Count >= 500)
            {
                await _importJobRepo.SaveChangesAsync(ct);
                _importJobRepo.DetachAndReattach(job);  // EF APIs stay in Infrastructure ✓
                job.Results.Clear();
            }
        }

        // ── Finalise job ────────────────────────────────────────────────────
        bool wasCancelled = await _importJobRepo.IsCancellationRequestedAsync(job.Id, ct);

        job.SuccessCount = successCount;
        job.FailureCount = failureCount;
        job.CompletedAt = DateTime.UtcNow;
        job.Status = wasCancelled
            ? ImportJobStatus.Cancelled
            : DetermineJobStatus(successCount, failureCount);

        await _importJobRepo.SaveChangesAsync(ct);

        // ── Clean up stored file (non-fatal on failure) ─────────────────────
        try
        {
            await _fileStore.DeleteAsync(job.CompanyId, job.FileStorageKey!, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Stored file cleanup failed (non-fatal). JobId={JobId} StorageKey={Key}",
                job.Id, job.FileStorageKey);
        }

        _logger.LogInformation(
            "Import completed. JobId={JobId} Status={Status} Success={S} Failure={F}",
            job.Id, job.Status, successCount, failureCount);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // CANCELLATION
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task CancelJobAsync(int jobId, CancellationToken ct = default)
    {
        bool updated = await _importJobRepo.RequestCancellationAsync(
            jobId, _currentUser.CompanyId, ct);

        if (!updated)
        {
            var job = await _importJobRepo.GetByIdAsync(jobId, _currentUser.CompanyId, ct);
            if (job is null) throw BusinessErrors.ImportJobNotFound(jobId);
            throw BusinessErrors.ImportJobNotCancellable(jobId, job.Status.ToString());
        }

        _logger.LogInformation(
            "Cancellation requested. JobId={JobId} UserId={UserId}", jobId, _currentUser.UserId);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // READ METHODS
    // ═══════════════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<ImportJobDto?> GetJobAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _importJobRepo.GetByIdAsync(jobId, _currentUser.CompanyId, ct);
        return job is null ? null : MapToDto(job);
    }

    /// <inheritdoc/>
    public async Task<ImportJobPagedResultDto?> GetJobResultsAsync(
        int jobId, int page = 1, int pageSize = 500, CancellationToken ct = default)
    {
        // Delegate paging and company scoping to the repository — no unbounded load.
        var paged = await _importJobRepo.GetPagedResultsAsync(
            jobId, _currentUser.CompanyId, page, pageSize, ct);

        if (paged is null) return null;

        var (job, results, totalCount) = paged.Value;

        return new ImportJobPagedResultDto
        {
            Id = job.Id,
            EntityType = job.EntityType,
            FileName = job.FileName,
            TotalRows = job.TotalRows,
            SuccessCount = job.SuccessCount,
            FailureCount = job.FailureCount,
            Status = job.Status,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            ErrorSummary = job.ErrorSummary,
            CreatedAt = job.CreatedAt,
            Page = Math.Max(page, 1),
            PageSize = Math.Clamp(pageSize, 1, 1000),
            TotalResultCount = totalCount,
            Results = results
                .Select(r => new ImportJobResultDto
                {
                    Id = r.Id,
                    RowNumber = r.RowNumber,
                    IsSuccess = r.IsSuccess,
                    ErrorMessage = r.ErrorMessage,
                    RawData = r.RawData
                })
                .ToList()
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ImportJobDto>> GetAllJobsAsync(
        string? entityType, CancellationToken ct = default)
    {
        var jobs = await _importJobRepo.GetAllByCompanyAsync(
            _currentUser.CompanyId, entityType, ct);
        return jobs.ConvertAll(MapToDto).AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task<byte[]> DownloadTemplateAsync(string entityType, CancellationToken ct = default)
    {
        ValidateEntityType(entityType.Trim().ToUpperInvariant(), entityType);
        return await _templateService.BuildTemplateCsvAsync(entityType, ct);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static void ValidateEntityType(string normalised, string original)
    {
        if (normalised is not ("PRODUCT" or "CATEGORY" or "UNITOFMEASURE"))
            throw BusinessErrors.ImportUnknownEntityType(original);
    }

    private static ImportJobStatus DetermineJobStatus(int successCount, int failureCount)
    {
        if (failureCount == 0) return ImportJobStatus.Completed;
        if (successCount == 0) return ImportJobStatus.Failed;
        return ImportJobStatus.CompletedWithErrors;
    }

    /// <summary>Strips path traversal components; preserves only the file name.</summary>
    private static string SanitizeFileName(string fileName) =>
        Path.GetFileName(fileName.Replace('\\', '/').Replace("..", string.Empty)) ?? "upload";

    private static ImportJobDto MapToDto(ImportJob job) => new()
    {
        Id = job.Id,
        EntityType = job.EntityType,
        FileName = job.FileName,
        TotalRows = job.TotalRows,
        SuccessCount = job.SuccessCount,
        FailureCount = job.FailureCount,
        Status = job.Status,
        StartedAt = job.StartedAt,
        CompletedAt = job.CompletedAt,
        ErrorSummary = job.ErrorSummary,
        CreatedAt = job.CreatedAt
    };
}