using ERPSystem.Infrastructure.Shared.PdfGeneration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration.Components
{
    /// <summary>
    /// Renders the page footer: optional notes, posted-by attribution, and page X of Y.
    /// Used in the document's Footer section so QuestPDF injects the correct page context.
    /// </summary>
    internal sealed class FooterComponent : IComponent
    {
        private readonly string? _notes;
        private readonly string? _postedByUserName;
        private readonly DateTime? _postedAt;
        private readonly string _lang;
        private readonly string _fontFamily;

        public FooterComponent(
            string? notes,
            string? postedByUserName,
            DateTime? postedAt,
            string lang,
            string fontFamily)
        {
            _notes = notes;
            _postedByUserName = postedByUserName;
            _postedAt = postedAt;
            _lang = lang;
            _fontFamily = fontFamily;
        }

        public void Compose(IContainer container)
        {
            var labels = PdfLabels.Get(_lang);
            var dateFormat = _lang == "ar" ? "dd/MM/yyyy HH:mm" : "MM/dd/yyyy HH:mm";

            container
                .BorderTop(1)
                .BorderColor("#e5e7eb")
                .Padding(8)
                .Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(_notes))
                    {
                        col.Item()
                            .Text($"{labels["notes"]}: {_notes}")
                            .FontFamily(_fontFamily)
                            .FontSize(9)
                            .FontColor("#374151");

                        col.Item().PaddingBottom(4);
                    }

                    col.Item().Row(row =>
                    {
                        // Posted-by attribution (left side)
                        row.RelativeItem().Column(inner =>
                        {
                            if (!string.IsNullOrWhiteSpace(_postedByUserName) && _postedAt.HasValue)
                            {
                                inner.Item()
                                    .Text($"{labels["postedBy"]}: {_postedByUserName}   {labels["postedAt"]}: {_postedAt.Value.ToString(dateFormat)}")
                                    .FontFamily(_fontFamily)
                                    .FontSize(8)
                                    .FontColor("#6b7280");
                            }
                        });

                        // Page X of Y (right side) — QuestPDF dynamic text
                        row.ConstantItem(120).AlignRight().Text(txt =>
                        {
                            txt.Span($"{labels["page"]} ")
                               .FontFamily(_fontFamily)
                               .FontSize(8)
                               .FontColor("#6b7280");
                            txt.CurrentPageNumber()
                               .FontFamily(_fontFamily)
                               .FontSize(8)
                               .FontColor("#6b7280");
                            txt.Span($" {labels["of"]} ")
                               .FontFamily(_fontFamily)
                               .FontSize(8)
                               .FontColor("#6b7280");
                            txt.TotalPages()
                               .FontFamily(_fontFamily)
                               .FontSize(8)
                               .FontColor("#6b7280");
                        });
                    });
                });
        }
    }
}