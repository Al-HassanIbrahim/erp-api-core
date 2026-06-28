using System.Globalization;
using System.Reflection;
using CsvHelper;
using CsvHelper.Configuration;
using ERPSystem.Application.DTOs.Import.Rows;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ExcelDataReader;

namespace ERPSystem.Application.Services.Import.Parsers;

// ═══════════════════════════════════════════════════════════════════════════════
// BASE PARSER
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Abstract base implementing <see cref="IFileParser{TRow}"/> for both CSV and XLSX formats.
/// </summary>
/// <remarks>
/// CSV uses CsvHelper (v30+) with streaming reads and case-insensitive header matching.
/// XLSX uses ExcelDataReader (v3.6+) — sequential streaming, no DOM buffering — safe for
/// large enterprise files. Reflection property map for XLSX is built once per call (not
/// per row) and keyed by normalised header name.
/// Subclasses may override <see cref="GetCsvClassMap"/> to supply a custom ClassMap.
/// </remarks>
public abstract class BaseImportFileParser<TRow> : IFileParser<TRow>
    where TRow : class, new()
{
    /// <inheritdoc/>
    public Task<List<TRow>> ParseAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        string ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".csv"  => ParseCsvAsync(fileStream, ct),
            ".xlsx" => ParseXlsxAsync(fileStream, ct),
            _ => throw BusinessErrors.ImportFileParseFailed(
                     $"Unsupported file extension '{ext}'. Supported formats: .csv, .xlsx.")
        };
    }

    /// <summary>
    /// Override to return a custom CsvHelper <see cref="ClassMap{TRow}"/>.
    /// Return <c>null</c> to use automatic header-based mapping.
    /// </summary>
    protected virtual ClassMap? GetCsvClassMap() => null;

    // ─── CSV ─────────────────────────────────────────────────────────────────

    private async Task<List<TRow>> ParseCsvAsync(Stream stream, CancellationToken ct)
    {
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter   = true,          // Handles comma, semicolon, tab
                HeaderValidated   = null,          // No throw on extra/missing headers
                MissingFieldFound = null,          // No throw on missing optional fields
                TrimOptions       = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.Trim().ToUpperInvariant()
            };

            using var reader = new StreamReader(stream, leaveOpen: true);
            using var csv    = new CsvReader(reader, config);

            ClassMap? classMap = GetCsvClassMap();
            if (classMap is not null)
                csv.Context.RegisterClassMap(classMap.GetType());

            var rows = new List<TRow>();

            // Consume the header row once before entering the data loop
            csv.Read();
            csv.ReadHeader();

            while (await csv.ReadAsync())
            {
                ct.ThrowIfCancellationRequested();
                var row = csv.GetRecord<TRow>();
                if (row is not null) rows.Add(row);
            }

            return rows;
        }
        catch (BusinessException) { throw; }
        catch (Exception ex)
        {
            throw BusinessErrors.ImportFileParseFailed($"CSV parsing error: {ex.Message}");
        }
    }

    // ─── XLSX ─────────────────────────────────────────────────────────────────

    private Task<List<TRow>> ParseXlsxAsync(Stream stream, CancellationToken ct)
    {
        // Required by ExcelDataReader on .NET Core for code-page encoding support
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        try
        {
            using var reader = ExcelReaderFactory.CreateReader(stream);

            // Row 0: header
            if (!reader.Read()) return Task.FromResult(new List<TRow>());

            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int col = 0; col < reader.FieldCount; col++)
            {
                string? h = reader.GetValue(col)?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(h)) headers.TryAdd(h, col);
            }

            // Build property lookup once — keyed by UPPER property name
            var propMap = typeof(TRow)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(p => p.Name.ToUpperInvariant());

            var rows = new List<TRow>();

            // Rows 1+: data
            while (reader.Read())
            {
                ct.ThrowIfCancellationRequested();

                // Skip entirely blank rows
                bool hasData = Enumerable.Range(0, reader.FieldCount)
                    .Any(c => reader.GetValue(c) is not null);
                if (!hasData) continue;

                var row = new TRow();
                foreach (var (headerName, colIdx) in headers)
                {
                    if (!propMap.TryGetValue(headerName.ToUpperInvariant(), out var prop)) continue;
                    object? cell = reader.GetValue(colIdx);
                    if (cell is null) continue;

                    try { prop.SetValue(row, ConvertCell(cell, prop.PropertyType)); }
                    catch { /* leave default; validator will catch */ }
                }
                rows.Add(row);
            }

            return Task.FromResult(rows);
        }
        catch (BusinessException) { throw; }
        catch (Exception ex)
        {
            throw BusinessErrors.ImportFileParseFailed($"XLSX parsing error: {ex.Message}");
        }
    }

    // ─── Cell type conversion ─────────────────────────────────────────────────

    private static object? ConvertCell(object cell, Type targetType)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        string s = cell.ToString()!.Trim();

        if (string.IsNullOrWhiteSpace(s))
            return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
                ? Activator.CreateInstance(targetType) : null;

        if (underlying == typeof(string))  return s;
        if (underlying == typeof(decimal)) return decimal.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(int))     return int.Parse(s, CultureInfo.InvariantCulture);
        if (underlying == typeof(bool))
        {
            if (bool.TryParse(s, out bool b)) return b;
            return s is "1" or "yes" or "true";
        }
        if (underlying == typeof(Guid)) return Guid.Parse(s);

        return Convert.ChangeType(cell, underlying, CultureInfo.InvariantCulture);
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// CONCRETE PARSERS
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// File parser for Product bulk import files.
/// Headers: Code | Name | Description | CategoryName | UnitOfMeasureName | DefaultPrice | MinQuantity | Barcode
/// </summary>
public sealed class ProductImportFileParser : BaseImportFileParser<ProductImportRow>
{
    /// <inheritdoc/>
    protected override ClassMap? GetCsvClassMap() => null;
}

/// <summary>
/// File parser for Category bulk import files.
/// Headers: Name | Description | ParentCategoryName
/// </summary>
public sealed class CategoryImportFileParser : BaseImportFileParser<CategoryImportRow>
{
    /// <inheritdoc/>
    protected override ClassMap? GetCsvClassMap() => null;
}

/// <summary>
/// File parser for Unit of Measure bulk import files.
/// Headers: Name | Symbol
/// </summary>
public sealed class UnitOfMeasureImportFileParser : BaseImportFileParser<UnitOfMeasureImportRow>
{
    /// <inheritdoc/>
    protected override ClassMap? GetCsvClassMap() => null;
}
