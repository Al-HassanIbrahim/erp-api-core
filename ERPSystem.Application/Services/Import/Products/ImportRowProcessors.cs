using ERPSystem.Application.DTOs.Import.Rows;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Application.Services.Import.Products;

// ═══════════════════════════════════════════════════════════════════════════════
// CRITICAL CONTRACT
// Processors MUST NOT call SaveChangesAsync.
// Transaction management (begin / SaveChanges / commit / rollback) is owned
// exclusively by ImportService.RunImportPipelineAsync via IUnitOfWork.
// This guarantees no phantom records: a failed row is never partially committed.
// ═══════════════════════════════════════════════════════════════════════════════

// ─── Product Processor ────────────────────────────────────────────────────────

/// <summary>
/// Stages a new <see cref="Product"/> entity in the EF change tracker for a validated
/// <see cref="ProductImportRow"/>. Does not call <c>SaveChangesAsync</c>.
/// </summary>
public sealed class ProductImportRowProcessor : IImportRowProcessor<ProductImportRow>
{
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfMeasureRepository _uomRepo;
    private readonly ICategoryRepository _categoryRepo;

    // Request-scoped caches: name → resolved Id (avoids N+1 per import)
    private readonly Dictionary<string, int> _uomCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _categoryCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initializes a new instance of <see cref="ProductImportRowProcessor"/>.</summary>
    public ProductImportRowProcessor(
        IProductRepository productRepo,
        IUnitOfMeasureRepository uomRepo,
        ICategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _uomRepo = uomRepo;
        _categoryRepo = categoryRepo;
    }

    /// <inheritdoc/>
    public async Task ProcessAsync(
        ProductImportRow row, int companyId, Guid actorUserId, CancellationToken ct = default)
    {
        // ── Resolve UoM Id (cached) ──────────────────────────────────────────
        string uomKey = row.UnitOfMeasureName.Trim();
        if (!_uomCache.TryGetValue(uomKey, out int uomId))
        {
            uomId = await _uomRepo.GetIdByNameAsync(uomKey, companyId, ct)
                ?? throw new BusinessException(
                    "IMPORT_UOM_NOT_FOUND",
                    $"Unit of measure '{uomKey}' was not found in this company.", 422);
            _uomCache[uomKey] = uomId;
        }

        // ── Resolve Category Id (cached, optional) ──────────────────────────
        int? categoryId = null;
        if (!string.IsNullOrWhiteSpace(row.CategoryName))
        {
            string catKey = row.CategoryName.Trim();
            if (!_categoryCache.TryGetValue(catKey, out int cachedCatId))
            {
                int? resolved = await _categoryRepo.GetIdByNameAsync(catKey, companyId, ct);
                if (resolved is null)
                    throw new BusinessException(
                        "IMPORT_CATEGORY_NOT_FOUND",
                        $"Category '{catKey}' was not found in this company.", 422);

                _categoryCache[catKey] = resolved.Value;
                cachedCatId = resolved.Value;
            }
            categoryId = cachedCatId;
        }

        // ── Stage for insert — caller commits ───────────────────────────────
        var product = new Product
        {
            CompanyId = companyId,
            Code = row.Code.Trim(),
            Name = row.Name.Trim(),
            Description = row.Description?.Trim(),
            CategoryId = categoryId,
            UnitOfMeasureId = uomId,
            DefaultPrice = row.DefaultPrice,
            MinQuantity = row.MinQuantity,
            Barcode = row.Barcode?.Trim(),
            IsActive = true,
            CreatedByUserId = actorUserId
        };

        await _productRepo.AddAsync(product);
        // ← NO SaveChangesAsync. ImportService owns the transaction.
    }
}

// ─── Category Processor ───────────────────────────────────────────────────────

/// <summary>
/// Stages a new <see cref="Category"/> entity for a validated <see cref="CategoryImportRow"/>.
/// Does not call <c>SaveChangesAsync</c>.
/// </summary>
/// <remarks>
/// <para>
/// Implements <see cref="IPostCommitNotifiable"/> so that <c>ImportService</c> can call
/// <see cref="NotifyCommitted"/> after each successful per-row transaction without
/// referencing this concrete type. EF Core only assigns the database PK after the INSERT
/// executes inside <c>SaveChangesAsync</c>. <see cref="NotifyCommitted"/> is called
/// post-commit so the cache always stores the real DB-assigned Id, allowing subsequent
/// rows to reference newly-imported categories as parents without an extra DB round-trip.
/// </para>
/// </remarks>
public sealed class CategoryImportRowProcessor
    : IImportRowProcessor<CategoryImportRow>, IPostCommitNotifiable
{
    private readonly ICategoryRepository _categoryRepo;

    // Populated via NotifyCommitted after each successful row commit.
    private readonly Dictionary<string, int> _categoryCache = new(StringComparer.OrdinalIgnoreCase);

    // Holds the last-staged entity so NotifyCommitted can read its DB-assigned Id post-commit.
    private Category? _lastStaged;

    /// <summary>Initializes a new instance of <see cref="CategoryImportRowProcessor"/>.</summary>
    public CategoryImportRowProcessor(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    /// <inheritdoc/>
    public async Task ProcessAsync(
        CategoryImportRow row, int companyId, Guid actorUserId, CancellationToken ct = default)
    {
        // ── Resolve parent category (cached) ────────────────────────────────
        int? parentId = null;
        if (!string.IsNullOrWhiteSpace(row.ParentCategoryName))
        {
            string parentKey = row.ParentCategoryName.Trim();
            if (!_categoryCache.TryGetValue(parentKey, out int cachedId))
            {
                int? resolved = await _categoryRepo.GetIdByNameAsync(parentKey, companyId, ct);
                if (resolved is null)
                    throw new BusinessException(
                        "IMPORT_CATEGORY_NOT_FOUND",
                        $"Parent category '{parentKey}' was not found in this company.", 422);

                _categoryCache[parentKey] = resolved.Value;
                cachedId = resolved.Value;
            }
            parentId = cachedId;
        }

        // ── Stage for insert ─────────────────────────────────────────────────
        var category = new Category
        {
            CompanyId = companyId,
            Name = row.Name.Trim(),
            Description = row.Description?.Trim(),
            ParentCategoryId = parentId,
            CreatedByUserId = actorUserId
        };

        await _categoryRepo.AddAsync(category, ct);
        // ← NO SaveChangesAsync. ImportService owns the transaction.

        _lastStaged = category;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Called by <c>ImportService</c> via <see cref="IPostCommitNotifiable"/> immediately
    /// after <c>IUnitOfWork.ExecuteInTransactionAsync</c> completes. At this point EF Core
    /// has flushed the INSERT and populated <c>category.Id</c> with the database-assigned PK.
    /// </remarks>
    public void NotifyCommitted()
    {
        if (_lastStaged is not null && _lastStaged.Id > 0)
        {
            _categoryCache.TryAdd(_lastStaged.Name, _lastStaged.Id);
            _lastStaged = null;
        }
    }
}

// ─── UnitOfMeasure Processor ──────────────────────────────────────────────────

/// <summary>
/// Stages a new <see cref="UnitOfMeasure"/> entity for a validated
/// <see cref="UnitOfMeasureImportRow"/>. Does not call <c>SaveChangesAsync</c>.
/// </summary>
public sealed class UnitOfMeasureImportRowProcessor : IImportRowProcessor<UnitOfMeasureImportRow>
{
    private readonly IUnitOfMeasureRepository _uomRepo;

    /// <summary>Initializes a new instance of <see cref="UnitOfMeasureImportRowProcessor"/>.</summary>
    public UnitOfMeasureImportRowProcessor(IUnitOfMeasureRepository uomRepo)
    {
        _uomRepo = uomRepo;
    }

    /// <inheritdoc/>
    public async Task ProcessAsync(
        UnitOfMeasureImportRow row, int companyId, Guid actorUserId, CancellationToken ct = default)
    {
        var uom = new UnitOfMeasure
        {
            CompanyId = companyId,
            Name = row.Name.Trim(),
            Symbol = row.Symbol.Trim(),
            CreatedByUserId = actorUserId
        };

        await _uomRepo.AddAsync(uom, ct);
        // ← NO SaveChangesAsync. ImportService owns the transaction.
    }
}