using ERPSystem.Domain.Entities.Core;
using ERPSystem.Infrastructure.Shared.PdfGeneration;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration.Components
{
    /// <summary>
    /// Renders the company header block at the top of every sales PDF.
    /// Displays a logo placeholder on the left, and company details on the right.
    /// </summary>
    internal sealed class CompanyHeaderComponent : IComponent
    {
        private readonly Company _company;
        private readonly string _lang;
        private readonly string _fontFamily;

        // Primary brand colour used for the header background bar.
        private const string PrimaryColor = "#1a56db";

        public CompanyHeaderComponent(Company company, string lang, string fontFamily)
        {
            _company = company;
            _lang = lang;
            _fontFamily = fontFamily;
        }

        public void Compose(IContainer container)
        {
            var labels = PdfLabels.Get(_lang);

            container
                .Background(PrimaryColor)
                .Padding(16)
                .Row(row =>
                {
                    // Left column: logo placeholder
                    row.ConstantItem(80).Column(col =>
                    {
                        col.Item()
                            .Width(64)
                            .Height(64)
                            .Background("#ffffff")
                            .AlignCenter()
                            .AlignMiddle()
                            .Text("LOGO")
                            .FontSize(10)
                            .FontColor("#1a56db");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item()
                            .Text(_company.Name)
                            .FontFamily(_fontFamily)
                            .FontSize(16)
                            .Bold()
                            .FontColor("#ffffff");

                        if (!string.IsNullOrWhiteSpace(_company.CommercialName))
                            col.Item()
                                .Text(_company.CommercialName)
                                .FontFamily(_fontFamily)
                                .FontSize(11)
                                .FontColor("#dbeafe");

                        if (!string.IsNullOrWhiteSpace(_company.TaxNumber))
                            col.Item()
                                .Text($"{labels["taxNumber"]}: {_company.TaxNumber}")
                                .FontFamily(_fontFamily)
                                .FontSize(10)
                                .FontColor("#dbeafe");

                        // Phone and address on one row when both exist
                        var contactParts = new List<string>();
                        if (!string.IsNullOrWhiteSpace(_company.Phone))
                            contactParts.Add($"{labels["phone"]}: {_company.Phone}");
                        if (!string.IsNullOrWhiteSpace(_company.Address))
                            contactParts.Add($"{labels["address"]}: {_company.Address}");

                        if (contactParts.Count > 0)
                            col.Item()
                                .Text(string.Join("  |  ", contactParts))
                                .FontFamily(_fontFamily)
                                .FontSize(9)
                                .FontColor("#dbeafe");
                    });
                });
        }
    }
}