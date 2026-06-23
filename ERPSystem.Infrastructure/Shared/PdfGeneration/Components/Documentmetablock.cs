using ERPSystem.Infrastructure.Shared.PdfGeneration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration.Components
{
    /// <summary>
    /// Renders the document metadata block: title, document number, date, status badge, and customer info.
    /// </summary>
    internal sealed class DocumentMetaBlock : IComponent
    {
        private readonly string _title;
        private readonly string _docNumber;
        private readonly DateTime _docDate;
        private readonly DateTime? _dueDate;
        private readonly string _statusLabel;
        private readonly string _statusColor;
        private readonly string _customerName;
        private readonly string _customerCode;
        private readonly string? _customerTaxNumber;
        private readonly string _lang;
        private readonly string _fontFamily;

        private const string PrimaryColor = "#1a56db";

        public DocumentMetaBlock(
            string title,
            string docNumber,
            DateTime docDate,
            DateTime? dueDate,
            string statusLabel,
            string statusColor,
            string customerName,
            string customerCode,
            string? customerTaxNumber,
            string lang,
            string fontFamily)
        {
            _title = title;
            _docNumber = docNumber;
            _docDate = docDate;
            _dueDate = dueDate;
            _statusLabel = statusLabel;
            _statusColor = statusColor;
            _customerName = customerName;
            _customerCode = customerCode;
            _customerTaxNumber = customerTaxNumber;
            _lang = lang;
            _fontFamily = fontFamily;
        }

        public void Compose(IContainer container)
        {
            var labels = PdfLabels.Get(_lang);
            var dateFormat = _lang == "ar" ? "dd/MM/yyyy" : "MM/dd/yyyy";

            container.Column(col =>
            {
                // Document title bar
                col.Item()
                    .Background(PrimaryColor)
                    .Padding(8)
                    .Text(_title)
                    .FontFamily(_fontFamily)
                    .FontSize(14)
                    .Bold()
                    .FontColor("#ffffff");

                // Document number / date / status row
                col.Item()
                    .BorderBottom(1)
                    .BorderColor("#e5e7eb")
                    .Padding(8)
                    .Row(row =>
                    {
                        // Doc number
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item()
                                .Text(labels["invoiceNumber"])
                                .FontFamily(_fontFamily)
                                .FontSize(8)
                                .FontColor("#6b7280");
                            inner.Item()
                                .Text(_docNumber)
                                .FontFamily(_fontFamily)
                                .FontSize(11)
                                .Bold();
                        });

                        // Doc date
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item()
                                .Text(labels["invoiceDate"])
                                .FontFamily(_fontFamily)
                                .FontSize(8)
                                .FontColor("#6b7280");
                            inner.Item()
                                .Text(_docDate.ToString(dateFormat))
                                .FontFamily(_fontFamily)
                                .FontSize(11);
                        });

                        // Due date (optional)
                        if (_dueDate.HasValue)
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item()
                                    .Text(labels["dueDate"])
                                    .FontFamily(_fontFamily)
                                    .FontSize(8)
                                    .FontColor("#6b7280");
                                inner.Item()
                                    .Text(_dueDate.Value.ToString(dateFormat))
                                    .FontFamily(_fontFamily)
                                    .FontSize(11);
                            });
                        }

                        // Status badge
                        row.ConstantItem(90).AlignRight().Column(inner =>
                        {
                            inner.Item()
                                .Background(_statusColor)
                                .Padding(4)
                                .AlignCenter()
                                .Text(_statusLabel)
                                .FontFamily(_fontFamily)
                                .FontSize(9)
                                .Bold()
                                .FontColor("#ffffff");
                        });
                    });

                // Customer row
                col.Item()
                    .Padding(8)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item()
                                .Text(labels["customer"])
                                .FontFamily(_fontFamily)
                                .FontSize(8)
                                .FontColor("#6b7280");
                            inner.Item()
                                .Text(_customerName)
                                .FontFamily(_fontFamily)
                                .FontSize(11)
                                .Bold();
                        });

                        row.RelativeItem().Column(inner =>
                        {
                            inner.Item()
                                .Text(labels["customerCode"])
                                .FontFamily(_fontFamily)
                                .FontSize(8)
                                .FontColor("#6b7280");
                            inner.Item()
                                .Text(_customerCode)
                                .FontFamily(_fontFamily)
                                .FontSize(11);
                        });

                        if (!string.IsNullOrWhiteSpace(_customerTaxNumber))
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item()
                                    .Text(labels["taxNumber"])
                                    .FontFamily(_fontFamily)
                                    .FontSize(8)
                                    .FontColor("#6b7280");
                                inner.Item()
                                    .Text(_customerTaxNumber)
                                    .FontFamily(_fontFamily)
                                    .FontSize(11);
                            });
                        }
                    });
            });
        }
    }
}