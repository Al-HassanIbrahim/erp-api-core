using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration.Components
{
    /// <summary>
    /// A generic line-items table that accepts column definitions and pre-formatted row data.
    /// All monetary values should already be formatted to "N2" before being passed in.
    /// </summary>
    internal sealed class ItemsTableComponent : IComponent
    {
        /// <summary>Column header label and relative width factor.</summary>
        public sealed record ColumnDef(string Header, float WidthFactor, bool RightAlign = false);

        private readonly IReadOnlyList<ColumnDef> _columns;
        private readonly IReadOnlyList<IReadOnlyList<string>> _rows;
        private readonly string _fontFamily;

        private const string HeaderBackground = "#1a56db";
        private const string AltRowBackground = "#f3f4f6";

        public ItemsTableComponent(
            IReadOnlyList<ColumnDef> columns,
            IReadOnlyList<IReadOnlyList<string>> rows,
            string fontFamily)
        {
            _columns = columns;
            _rows = rows;
            _fontFamily = fontFamily;
        }

        public void Compose(IContainer container)
        {
            container.Table(table =>
            {
                // Column widths
                table.ColumnsDefinition(cols =>
                {
                    foreach (var col in _columns)
                        cols.RelativeColumn(col.WidthFactor);
                });

                // Header row
                table.Header(header =>
                {
                    foreach (var col in _columns)
                    {
                        header.Cell()
                            .Background(HeaderBackground)
                            .Padding(6)
                            .Text(col.Header)
                            .FontFamily(_fontFamily)
                            .FontSize(9)
                            .Bold()
                            .FontColor("#ffffff");
                    }
                });

                // Data rows
                for (int rowIdx = 0; rowIdx < _rows.Count; rowIdx++)
                {
                    var rowData = _rows[rowIdx];
                    var rowBg = rowIdx % 2 == 0 ? "#ffffff" : AltRowBackground;

                    for (int colIdx = 0; colIdx < _columns.Count && colIdx < rowData.Count; colIdx++)
                    {
                        var colDef = _columns[colIdx];
                        var cell = table.Cell()
                            .Background(rowBg)
                            .BorderBottom(1)
                            .BorderColor("#e5e7eb")
                            .Padding(5);

                        if (colDef.RightAlign)
                            cell.AlignRight()
                                .Text(rowData[colIdx])
                                .FontFamily(_fontFamily)
                                .FontSize(9);
                        else
                            cell.Text(rowData[colIdx])
                                .FontFamily(_fontFamily)
                                .FontSize(9);
                    }
                }
            });
        }
    }
}