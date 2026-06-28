using System.Text;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;

namespace ERPSystem.Application.Services.Import;

/// <summary>
/// Generates downloadable CSV import templates for all entity types
/// supported by the Bulk Import subsystem.
/// </summary>
public sealed class ImportTemplateService : IImportTemplateService
{
    private static readonly string[] ProductHeaders =
        ["Code", "Name", "Description", "CategoryName", "UnitOfMeasureName", "DefaultPrice", "MinQuantity", "Barcode"];

    private static readonly string[] CategoryHeaders =
        ["Name", "Description", "ParentCategoryName"];

    private static readonly string[] UnitOfMeasureHeaders =
        ["Name", "Symbol"];

    /// <inheritdoc/>
    public string[] GetHeaders(string entityType) =>
        entityType.Trim().ToUpperInvariant() switch
        {
            "PRODUCT"       => ProductHeaders,
            "CATEGORY"      => CategoryHeaders,
            "UNITOFMEASURE" => UnitOfMeasureHeaders,
            _               => throw BusinessErrors.ImportUnknownEntityType(entityType)
        };

    /// <inheritdoc/>
    public Task<byte[]> BuildTemplateCsvAsync(string entityType, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        string[] headers = GetHeaders(entityType);

        // UTF-8 BOM so Excel opens the file with correct encoding automatically.
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

        writer.WriteLine(string.Join(",", headers.Select(EscapeCsvField)));
        writer.Flush();

        return Task.FromResult(ms.ToArray());
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";
        return field;
    }
}
