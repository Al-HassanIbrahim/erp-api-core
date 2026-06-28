using ERPSystem.Application.Authorization;
using ERPSystem.Application.DTOs.Import;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSystem.API.Controllers;

/// <summary>
/// Exposes all Bulk Import endpoints:
/// upload, status polling, row-level results, cancellation, and template download.
/// </summary>
/// <remarks>
/// <para>
/// <b>Upload flow:</b> POST returns <c>202 Accepted</c> immediately with a <c>jobId</c>.
/// The actual pipeline runs in a Hangfire background worker. Clients poll
/// <c>GET /api/import/{jobId}</c> until <c>Status</c> is a terminal value
/// (Completed, CompletedWithErrors, Failed, Cancelled).
/// </para>
/// <para>All endpoints are scoped to the authenticated user's <c>CompanyId</c>.</para>
/// </remarks>
[ApiController]
[Route("api/import")]
public sealed class ImportController : ControllerBase
{
    // Max upload size enforced at ASP.NET Core level before the body is read.
    // Must match ImportService.MaxFileSizeBytes (50 MB).
    private const long MaxUploadBytes = 50L * 1024 * 1024;

    private readonly IImportService _importService;

    /// <summary>Initializes a new instance of <see cref="ImportController"/>.</summary>
    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    // ─── POST /api/import/{entityType} ───────────────────────────────────────

    /// <summary>
    /// Accepts a CSV or XLSX file upload, persists a Pending import job, stores the
    /// file bytes, and enqueues the background processing worker.
    /// Returns <c>202 Accepted</c> immediately with the <c>jobId</c> and a poll URL.
    /// </summary>
    /// <param name="entityType">
    /// Entity type to import: <c>Product</c>, <c>Category</c>, or <c>UnitOfMeasure</c>
    /// (case-insensitive).
    /// </param>
    /// <param name="file">
    /// The uploaded file (multipart/form-data). Max 50 MB. Must be <c>.csv</c> or <c>.xlsx</c>.
    /// </param>
    /// <param name="ct">Cancellation token from the HTTP request.</param>
    /// <returns>
    /// <c>202 Accepted</c> with <c>{ jobId, pollUrl }</c>.
    /// Poll <c>GET /api/import/{jobId}</c> to track progress.
    /// </returns>
    /// <remarks>
    /// Clients may send an <c>Idempotency-Key</c> request header (UUID recommended) to
    /// prevent duplicate submissions. A second request with the same key returns
    /// <c>409 Conflict</c> with the existing job ID.
    /// </remarks>
    /// <response code="202">Import enqueued. Poll the returned URL for progress.</response>
    /// <response code="400">No file, unsupported extension, file too large, or unknown entity type.</response>
    /// <response code="409">Duplicate idempotency key — existing job ID returned.</response>
    /// <response code="403">Missing <c>Import.Jobs.Manage</c> permission.</response>
    [HttpPost("{entityType}")]
    [Authorize(Policy = Permissions.Import.Jobs.Manage)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> EnqueueImportAsync(
        [FromRoute] string entityType,
        IFormFile file,
        CancellationToken ct)
    {
        // ── Guard: file present ───────────────────────────────────────────────
        if (file is null || file.Length == 0)
            return BadRequest(new
            {
                error = new { code = "IMPORT_NO_FILE", message = "No file was uploaded." }
            });

        // ── Read bytes and dispose stream before any async boundary ───────────
        // This eliminates the stream-lifetime bug from v1: no open stream crosses
        // the service boundary. The byte array is safe for background job handoff.
        byte[] fileBytes;
        await using (var stream = file.OpenReadStream())
        {
            using var ms = new MemoryStream((int)Math.Min(file.Length, MaxUploadBytes));
            await stream.CopyToAsync(ms, ct);
            fileBytes = ms.ToArray();
        }
        // stream is fully disposed here ────────────────────────────────────────

        // ── Optional idempotency key from request header ──────────────────────
        string? idempotencyKey = Request.Headers.TryGetValue("Idempotency-Key", out var vals)
            ? vals.ToString()
            : null;

        var request = new ImportRequest
        {
            EntityType     = entityType,
            FileBytes      = fileBytes,
            FileName       = file.FileName,
            IdempotencyKey = idempotencyKey
        };

        int jobId = await _importService.EnqueueImportAsync(request, ct);

        string pollUrl = Url.Action(nameof(GetJobAsync), new { jobId })!;

        return Accepted(pollUrl, new { jobId, pollUrl });
    }

    // ─── DELETE /api/import/{jobId}/cancel ───────────────────────────────────

    /// <summary>
    /// Requests cancellation of a Pending or Processing import job.
    /// The background worker stops at the next inter-row checkpoint.
    /// Rows already committed to the database are not rolled back.
    /// </summary>
    /// <param name="jobId">The import job primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>204 No Content</c> on success;
    /// <c>404</c> if the job is not found;
    /// <c>409</c> if the job is already in a terminal state.
    /// </returns>
    /// <response code="204">Cancellation accepted.</response>
    /// <response code="404">Job not found for the current tenant.</response>
    /// <response code="409">Job is already completed, failed, or cancelled.</response>
    /// <response code="403">Missing <c>Import.Jobs.Manage</c> permission.</response>
    [HttpDelete("{jobId:int}/cancel")]
    [Authorize(Policy = Permissions.Import.Jobs.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelJobAsync(
        [FromRoute] int jobId,
        CancellationToken ct)
    {
        await _importService.CancelJobAsync(jobId, ct);
        return NoContent();
    }

    // ─── GET /api/import ─────────────────────────────────────────────────────

    /// <summary>
    /// Lists all import jobs for the current tenant, ordered by creation date descending.
    /// </summary>
    /// <param name="entityType">
    /// Optional filter (e.g., <c>Product</c>). Case-insensitive. Omit for all types.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>200 OK</c> with a list of <see cref="ImportJobDto"/> summaries.</returns>
    /// <response code="200">Returns the job list (may be empty).</response>
    /// <response code="403">Missing <c>Import.Jobs.Read</c> permission.</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Import.Jobs.Read)]
    [ProducesResponseType(typeof(IReadOnlyList<ImportJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllJobsAsync(
        [FromQuery] string? entityType,
        CancellationToken ct)
    {
        var jobs = await _importService.GetAllJobsAsync(entityType, ct);
        return Ok(jobs);
    }

    // ─── GET /api/import/{jobId} ─────────────────────────────────────────────

    /// <summary>
    /// Returns the summary of a single import job. Poll this endpoint after upload
    /// to track background processing progress.
    /// </summary>
    /// <param name="jobId">The import job primary key.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>200 OK</c> with <see cref="ImportJobDto"/>, or <c>404 Not Found</c>.
    /// </returns>
    /// <response code="200">Returns the import job summary.</response>
    /// <response code="403">Missing <c>Import.Jobs.Read</c> permission.</response>
    /// <response code="404">Job not found for the current tenant.</response>
    [HttpGet("{jobId:int}")]
    [Authorize(Policy = Permissions.Import.Jobs.Read)]
    [ProducesResponseType(typeof(ImportJobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobAsync(
        [FromRoute] int jobId,
        CancellationToken ct)
    {
        var job = await _importService.GetJobAsync(jobId, ct);
        return job is null ? NotFound() : Ok(job);
    }

    // ─── GET /api/import/{jobId}/results ─────────────────────────────────────

    /// <summary>
    /// Returns a page of per-row results for the given import job.
    /// Use <c>page</c> and <c>pageSize</c> query parameters to navigate large result sets.
    /// </summary>
    /// <param name="jobId">The import job primary key.</param>
    /// <param name="page">
    /// 1-based page number. Defaults to <c>1</c>.
    /// </param>
    /// <param name="pageSize">
    /// Number of result rows per page. Defaults to <c>500</c>. Server-clamped to [1, 1000].
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>200 OK</c> with <see cref="ImportJobPagedResultDto"/> including pagination metadata,
    /// or <c>404 Not Found</c>.
    /// </returns>
    /// <remarks>
    /// The response includes an <c>X-Pagination</c> HTTP header with serialized pagination
    /// metadata (totalCount, totalPages, currentPage, pageSize) so clients do not need to
    /// parse the body to decide whether to fetch the next page.
    ///
    /// Example usage: fetch pages until <c>page &gt; totalPages</c>.
    /// </remarks>
    /// <response code="200">Returns the paged import results.</response>
    /// <response code="403">Missing <c>Import.Jobs.Read</c> permission.</response>
    /// <response code="404">Job not found for the current tenant.</response>
    [HttpGet("{jobId:int}/results")]
    [Authorize(Policy = Permissions.Import.Jobs.Read)]
    [ProducesResponseType(typeof(ImportJobPagedResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobResultsAsync(
        [FromRoute] int jobId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 500,
        CancellationToken ct = default)
    {
        var detail = await _importService.GetJobResultsAsync(jobId, page, pageSize, ct);

        if (detail is null) return NotFound();

        // Expose pagination metadata in a response header so clients can paginate without
        // parsing the body. Uses the same page/pageSize that were clamped server-side
        // (reflected back in the DTO) so the header is always consistent with the body.
        int totalPages = detail.TotalResultCount == 0
            ? 0
            : (int)Math.Ceiling(detail.TotalResultCount / (double)detail.PageSize);

        Response.Headers.Append("X-Pagination",
            System.Text.Json.JsonSerializer.Serialize(new
            {
                totalCount  = detail.TotalResultCount,
                totalPages,
                currentPage = detail.Page,
                pageSize    = detail.PageSize
            }));

        return Ok(detail);
    }

    // ─── GET /api/import/template/{entityType} ────────────────────────────────

    /// <summary>
    /// Downloads a blank UTF-8 BOM CSV template for the specified entity type.
    /// The template contains only the header row with the expected column names.
    /// </summary>
    /// <param name="entityType">
    /// <c>Product</c>, <c>Category</c>, or <c>UnitOfMeasure</c> (case-insensitive).
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>200 OK</c> with the CSV file for download, or <c>400</c> for unknown entity type.
    /// </returns>
    /// <response code="200">Returns the CSV template file.</response>
    /// <response code="400">Unknown entity type.</response>
    /// <response code="403">Missing <c>Import.Jobs.Read</c> permission.</response>
    [HttpGet("template/{entityType}")]
    [Authorize(Policy = Permissions.Import.Jobs.Read)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadTemplateAsync(
        [FromRoute] string entityType,
        CancellationToken ct)
    {
        byte[] csv = await _importService.DownloadTemplateAsync(entityType, ct);
        string fileName = $"import-template-{entityType.Trim().ToLowerInvariant()}.csv";
        return File(csv, "text/csv; charset=utf-8", fileName);
    }
}
