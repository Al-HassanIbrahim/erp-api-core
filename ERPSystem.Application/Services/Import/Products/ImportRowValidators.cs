using ERPSystem.Application.DTOs.Import.Rows;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Application.Services.Import.Products;

// ═══════════════════════════════════════════════════════════════════════════════
// PRODUCT VALIDATOR
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Validates a single <see cref="ProductImportRow"/> against all business rules
/// before it reaches the processor layer.
/// </summary>
/// <remarks>
/// Rules (evaluated sequentially, fail-fast per row):
/// <list type="number">
///   <item>Code is required.</item>
///   <item>Name is required.</item>
///   <item>UnitOfMeasureName is required.</item>
///   <item>DefaultPrice must be ≥ 0.</item>
///   <item>Code must not already exist in this company.</item>
///   <item>The referenced UnitOfMeasure must exist in this company.</item>
/// </list>
/// UoM existence checks are cached per import to avoid N+1 queries.
/// </remarks>
public sealed class ProductImportRowValidator : IImportRowValidator<ProductImportRow>
{
    private readonly IProductRepository       _productRepo;
    private readonly IUnitOfMeasureRepository _uomRepo;

    // Request-scoped cache: UoM name → exists flag
    private readonly Dictionary<string, bool> _uomCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initializes a new instance of <see cref="ProductImportRowValidator"/>.</summary>
    public ProductImportRowValidator(
        IProductRepository productRepo,
        IUnitOfMeasureRepository uomRepo)
    {
        _productRepo = productRepo;
        _uomRepo     = uomRepo;
    }

    /// <inheritdoc/>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(
        ProductImportRow row, int rowNumber, int companyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(row.Code))
            return Fail(rowNumber, "Code is required.");

        if (string.IsNullOrWhiteSpace(row.Name))
            return Fail(rowNumber, "Name is required.");

        if (string.IsNullOrWhiteSpace(row.UnitOfMeasureName))
            return Fail(rowNumber, "UnitOfMeasureName is required.");

        if (row.DefaultPrice < 0)
            return Fail(rowNumber, $"DefaultPrice must be ≥ 0 (was {row.DefaultPrice}).");

        if (row.MinQuantity.HasValue && row.MinQuantity.Value < 0)
            return Fail(rowNumber, $"MinQuantity must be ≥ 0 (was {row.MinQuantity.Value}).");

        // Code uniqueness check (DB round-trip per row — cannot be cached across rows)
        if (await _productRepo.CodeExistsAsync(row.Code.Trim(), companyId))
            return Fail(rowNumber, $"Product code '{row.Code.Trim()}' already exists in this company.");

        // UoM existence (cached)
        string uomKey = row.UnitOfMeasureName.Trim();
        if (!_uomCache.TryGetValue(uomKey, out bool uomExists))
        {
            uomExists = await _uomRepo.ExistsByNameAsync(uomKey, companyId, ct);
            _uomCache[uomKey] = uomExists;
        }

        if (!uomExists)
            return Fail(rowNumber, $"Unit of measure '{uomKey}' does not exist in this company.");

        return (true, null);
    }

    private static (bool, string) Fail(int row, string msg) => (false, $"Row {row}: {msg}");
}

// ═══════════════════════════════════════════════════════════════════════════════
// CATEGORY VALIDATOR
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Validates a single <see cref="CategoryImportRow"/> before it reaches the processor.
/// </summary>
/// <remarks>
/// Rules: Name is required (non-empty after trim).
/// </remarks>
public sealed class CategoryImportRowValidator : IImportRowValidator<CategoryImportRow>
{
    /// <inheritdoc/>
    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(
        CategoryImportRow row, int rowNumber, int companyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
            return Task.FromResult<(bool, string?)>((false, $"Row {rowNumber}: Name is required."));

        return Task.FromResult<(bool, string?)>((true, null));
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// UNIT OF MEASURE VALIDATOR
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Validates a single <see cref="UnitOfMeasureImportRow"/> before it reaches the processor.
/// </summary>
/// <remarks>
/// Rules: Name required, Symbol required.
/// </remarks>
public sealed class UnitOfMeasureImportRowValidator : IImportRowValidator<UnitOfMeasureImportRow>
{
    /// <inheritdoc/>
    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(
        UnitOfMeasureImportRow row, int rowNumber, int companyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
            return Task.FromResult<(bool, string?)>((false, $"Row {rowNumber}: Name is required."));

        if (string.IsNullOrWhiteSpace(row.Symbol))
            return Task.FromResult<(bool, string?)>((false, $"Row {rowNumber}: Symbol is required."));

        return Task.FromResult<(bool, string?)>((true, null));
    }
}
