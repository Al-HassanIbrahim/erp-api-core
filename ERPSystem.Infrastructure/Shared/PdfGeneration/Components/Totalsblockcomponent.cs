using ERPSystem.Infrastructure.Shared.PdfGeneration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration.Components
{
    /// <summary>
    /// Renders the financial totals section: subtotal, discount, tax, grand total, paid amount, balance due.
    /// Pass null for optional fields (e.g. paidAmount / balanceDue on delivery or return documents).
    /// </summary>
    internal sealed class TotalsBlockComponent : IComponent
    {
        private readonly decimal _subTotal;
        private readonly decimal _discountAmount;
        private readonly decimal _taxAmount;
        private readonly decimal _grandTotal;
        private readonly decimal? _paidAmount;
        private readonly decimal? _balanceDue;
        private readonly string _lang;
        private readonly string _fontFamily;

        private const string AccentRed = "#e02424";

        public TotalsBlockComponent(
            decimal subTotal,
            decimal discountAmount,
            decimal taxAmount,
            decimal grandTotal,
            decimal? paidAmount,
            decimal? balanceDue,
            string lang,
            string fontFamily)
        {
            _subTotal = subTotal;
            _discountAmount = discountAmount;
            _taxAmount = taxAmount;
            _grandTotal = grandTotal;
            _paidAmount = paidAmount;
            _balanceDue = balanceDue;
            _lang = lang;
            _fontFamily = fontFamily;
        }

        public void Compose(IContainer container)
        {
            var labels = PdfLabels.Get(_lang);

            // Right-align the totals block to a fixed-width column
            container.AlignRight().Width(260).Column(col =>
            {
                TotalsRow(col, labels["subTotal"], _subTotal.ToString("N2"), bold: false);
                TotalsRow(col, labels["discountAmount"], $"-{_discountAmount.ToString("N2")}", bold: false);
                TotalsRow(col, labels["taxAmount"], $"+{_taxAmount.ToString("N2")}", bold: false);

                // Grand total separator
                col.Item().BorderTop(1).BorderColor("#374151").Padding(0);

                TotalsRow(col, labels["grandTotal"], _grandTotal.ToString("N2"), bold: true);

                if (_paidAmount.HasValue)
                    TotalsRow(col, labels["paidAmount"], $"-{_paidAmount.Value.ToString("N2")}", bold: false);

                if (_balanceDue.HasValue)
                    TotalsRow(col, labels["balanceDue"], _balanceDue.Value.ToString("N2"), bold: true, color: AccentRed);
            });
        }

        private void TotalsRow(ColumnDescriptor col, string label, string value, bool bold, string color = "#111827")
        {
            col.Item().Row(row =>
            {
                var labelText = row.RelativeItem()
                    .Padding(4)
                    .Text(label)
                    .FontFamily(_fontFamily)
                    .FontSize(10)
                    .FontColor(color);

                if (bold)
                    labelText.Bold();

                var valueText = row.ConstantItem(100)
                    .AlignRight()
                    .Padding(4)
                    .Text(value)
                    .FontFamily(_fontFamily)
                    .FontSize(10)
                    .FontColor(color);

                if (bold)
                    valueText.Bold();
            });
        }
    }
}