namespace ERPSystem.Application.DTOs.Import.Rows;

// ─── ProductImportRow ─────────────────────────────────────────────────────────

/// <summary>
/// Represents a single data row parsed from a Product bulk import file.
/// Column headers in CSV/XLSX files must match property names (case-insensitive).
/// </summary>
/// <remarks>
/// Expected headers: Code | Name | Description | CategoryName | UnitOfMeasureName |
/// DefaultPrice | MinQuantity | Barcode
/// </remarks>
public sealed class ProductImportRow
{
    /// <summary>Internal product code (SKU). Required. Must be unique within the company.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Product display name. Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional product description.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category name resolved by exact match within the tenant company.
    /// Optional; the category must exist before importing if provided.
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// Unit of measure name resolved by exact match within the tenant company. Required.
    /// </summary>
    public string UnitOfMeasureName { get; set; } = string.Empty;

    /// <summary>Baseline selling price. Required. Must be ≥ 0.</summary>
    public decimal DefaultPrice { get; set; }

    /// <summary>Optional low-stock alert threshold quantity.</summary>
    public decimal? MinQuantity { get; set; }

    /// <summary>Optional barcode (EAN, UPC, or custom format).</summary>
    public string? Barcode { get; set; }
}

// ─── CategoryImportRow ────────────────────────────────────────────────────────

/// <summary>
/// Represents a single data row parsed from a Category bulk import file.
/// </summary>
/// <remarks>Expected headers: Name | Description | ParentCategoryName</remarks>
public sealed class CategoryImportRow
{
    /// <summary>Category display name. Required.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Optional category description.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional parent category name resolved by exact match.
    /// The parent must exist before importing. Leave blank for top-level categories.
    /// </summary>
    public string? ParentCategoryName { get; set; }
}

// ─── UnitOfMeasureImportRow ───────────────────────────────────────────────────

/// <summary>
/// Represents a single data row parsed from a Unit of Measure bulk import file.
/// </summary>
/// <remarks>Expected headers: Name | Symbol</remarks>
public sealed class UnitOfMeasureImportRow
{
    /// <summary>Unit of measure display name. Required. Example: "Kilogram".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short symbol. Required. Example: "kg".</summary>
    public string Symbol { get; set; } = string.Empty;
}
