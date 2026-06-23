using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Core;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;
using ERPSystem.Infrastructure.Shared.PdfGeneration.Components;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration
{
    /// <summary>
    /// Generates PDF documents for all Sales module document types using QuestPDF.
    /// Font registration must be performed once at application startup before this service is used.
    /// </summary>
    public sealed class PdfExportService : IPdfExportService
    {
        private readonly ISalesInvoiceRepository _invoiceRepo;
        private readonly ISalesDeliveryRepository _deliveryRepo;
        private readonly ISalesReceiptRepository _receiptRepo;
        private readonly ISalesReturnRepository _returnRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        // Font family names — must match names registered via FontManager at startup
        private const string FontArabic = "Noto Sans Arabic";
        private const string FontDefault = "Noto Sans";

        // Brand colours
        private const string StatusGreen = "#057a55";
        private const string StatusOrange = "#d03801";
        private const string StatusRed = "#e02424";
        private const string WatermarkGray = "#9ca3af";

        public PdfExportService(
            ISalesInvoiceRepository invoiceRepo,
            ISalesDeliveryRepository deliveryRepo,
            ISalesReceiptRepository receiptRepo,
            ISalesReturnRepository returnRepo,
            ICompanyRepository companyRepo,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _invoiceRepo = invoiceRepo;
            _deliveryRepo = deliveryRepo;
            _receiptRepo = receiptRepo;
            _returnRepo = returnRepo;
            _companyRepo = companyRepo;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        public async Task<byte[]> GenerateSalesInvoicePdfAsync(int invoiceId, string lang, CancellationToken ct = default)
        {
            lang = lang.ToLowerInvariant();
            await _moduleAccess.EnsureSalesEnabledAsync(ct);

            var companyId = _currentUser.CompanyId;
            var company = await LoadCompanyAsync(companyId, ct);
            var invoice = await _invoiceRepo.GetByIdWithDetailsAsync(invoiceId, companyId, ct)
                            ?? throw BusinessErrors.SalesInvoiceNotFound(invoiceId);

            return GenerateDocument(lang, doc =>
            {
                var fontFamily = lang == "ar" ? FontArabic : FontDefault;
                var labels = PdfLabels.Get(lang);
                var isDraft = invoice.Status == SalesInvoiceStatus.Draft;
                var dateFormat = lang == "ar" ? "dd/MM/yyyy" : "MM/dd/yyyy";

                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(txt => txt.FontFamily(fontFamily));

                    // Handle text and layout direction based on the language
                    if (lang == "ar")
                    {
                        page.ContentFromRightToLeft();
                    }
                    else
                    {
                        page.ContentFromLeftToRight();
                    }

                    // ---- Header Component ----
                    page.Header().Component(new CompanyHeaderComponent(company, lang, fontFamily));

                    // ---- Content Body ----
                    page.Content().PaddingTop(8).Column(col =>
                    {
                        // Document meta block
                        col.Item().Component(new DocumentMetaBlock(
                            title: labels["invoiceTitle"],
                            docNumber: invoice.InvoiceNumber,
                            docDate: invoice.InvoiceDate,
                            dueDate: invoice.DueDate,
                            statusLabel: LocalizeInvoiceStatus(invoice.Status, labels),
                            statusColor: InvoiceStatusColor(invoice.Status),
                            customerName: invoice.Customer?.Name ?? "N/A", // Defensive approach against nulls
                            customerCode: invoice.Customer?.Code ?? "N/A",
                            customerTaxNumber: invoice.Customer?.TaxNumber,
                            lang: lang,
                            fontFamily: fontFamily));

                        col.Item().PaddingTop(12);

                        // Line items table configuration
                        var columns = new List<ItemsTableComponent.ColumnDef>
            {
                new(labels["no"],        0.5f),
                new(labels["product"],   3.0f),
                new(labels["unit"],      1.0f),
                new(labels["quantity"],  1.0f, RightAlign: true),
                new(labels["unitPrice"], 1.2f, RightAlign: true),
                new(labels["discount"],  1.0f, RightAlign: true),
                new(labels["tax"],       1.0f, RightAlign: true),
                new(labels["lineTotal"], 1.2f, RightAlign: true),
            };

                        var rows = new List<IReadOnlyList<string>>();
                        int lineNum = 1;
                        foreach (var line in invoice.Lines)
                        {
                            rows.Add(new[]
                            {
                    lineNum++.ToString(),
                    line.Product?.Name ?? string.Empty,
                    line.Unit?.Name    ?? string.Empty,
                    line.Quantity.ToString("N2"),
                    line.UnitPrice.ToString("N2"),
                    line.DiscountAmount.ToString("N2"),
                    line.TaxAmount.ToString("N2"),
                    line.LineTotal.ToString("N2"),
                });
                        }

                        col.Item().Component(new ItemsTableComponent(columns, rows, fontFamily));

                        col.Item().PaddingTop(12);

                        // Totals configuration
                        var balanceDue = invoice.GrandTotal - invoice.PaidAmount;
                        col.Item().Component(new TotalsBlockComponent(
                            subTotal: invoice.SubTotal,
                            discountAmount: invoice.DiscountAmount,
                            taxAmount: invoice.TaxAmount,
                            grandTotal: invoice.GrandTotal,
                            paidAmount: invoice.PaidAmount,
                            balanceDue: balanceDue,
                            lang: lang,
                            fontFamily: fontFamily));

                        // Payment status label
                        col.Item().PaddingTop(6).AlignRight()
                            .Text($"{labels["paymentStatus"]}: {LocalizePaymentStatus(invoice.PaymentStatus, labels)}")
                            .FontFamily(fontFamily)
                            .FontSize(10)
                            .FontColor(PaymentStatusColor(invoice.PaymentStatus))
                            .Bold();

                        // Render watermark if document is in draft mode
                        if (isDraft)
                            RenderDraftWatermark(col, labels["draftWatermark"], fontFamily);
                    });

                    // ---- Footer Component ----
                    page.Footer().Component(new FooterComponent(
                        notes: invoice.Notes,
                        postedByUserName: null,
                        postedAt: invoice.PostedAt,
                        lang: lang,
                        fontFamily: fontFamily));
                });
            });
        }


        public async Task<byte[]> GenerateSalesDeliveryPdfAsync(int deliveryId, string lang, CancellationToken ct = default)
        {
            lang = lang.ToLowerInvariant();
            await _moduleAccess.EnsureSalesEnabledAsync(ct);

            var companyId = _currentUser.CompanyId;
            var company = await LoadCompanyAsync(companyId, ct);
            var delivery = await _deliveryRepo.GetByIdWithDetailsAsync(deliveryId, companyId, ct)
                            ?? throw BusinessErrors.SalesDeliveryNotFound(deliveryId);

            return GenerateDocument(lang, doc =>
            {
                var fontFamily = lang == "ar" ? FontArabic : FontDefault;
                var labels = PdfLabels.Get(lang);
                var isDraft = delivery.Status == SalesDeliveryStatus.Draft;
                var dateFormat = lang == "ar" ? "dd/MM/yyyy" : "MM/dd/yyyy";

                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(txt => txt.FontFamily(fontFamily));

                    if (lang == "ar")
                    {
                        page.ContentFromRightToLeft();
                    }
                    else
                    {
                        page.ContentFromLeftToRight();
                    }

                    page.Header().Component(new CompanyHeaderComponent(company, lang, fontFamily));

                    page.Content().PaddingTop(8).Column(col =>
                    {
                        col.Item().Component(new DocumentMetaBlock(
                            title: labels["deliveryTitle"],
                            docNumber: delivery.DeliveryNumber,
                            docDate: delivery.DeliveryDate,
                            dueDate: null,
                            statusLabel: LocalizeDeliveryStatus(delivery.Status, labels),
                            statusColor: GenericStatusColor((int)delivery.Status),
                            customerName: delivery.Customer?.Name ?? "N/A",
                            customerCode: delivery.Customer?.Code ?? "N/A",
                            customerTaxNumber: delivery.Customer?.TaxNumber,
                            lang: lang,
                            fontFamily: fontFamily));

                        // Warehouse info
                        col.Item().PaddingTop(6).PaddingBottom(6)
                            .Text($"{labels["warehouse"]}: {delivery.Warehouse?.Name ?? "N/A"}")
                            .FontFamily(fontFamily)
                            .FontSize(10);

                        // Reference invoice
                        if (delivery.SalesInvoice is not null)
                            col.Item().PaddingBottom(6)
                                .Text($"{labels["invoiceNumber"]}: {delivery.SalesInvoice.InvoiceNumber}")
                                .FontFamily(fontFamily)
                                .FontSize(10);

                        col.Item().PaddingTop(4);

                        var columns = new List<ItemsTableComponent.ColumnDef>
                        {
                            new(labels["no"],       0.5f),
                            new(labels["product"],  3.0f),
                            new(labels["unit"],     1.0f),
                            new(labels["quantity"], 1.2f, RightAlign: true),
                        };

                        var rows = new List<IReadOnlyList<string>>();
                        int lineNum = 1;
                        foreach (var line in delivery.Lines)
                        {
                            rows.Add(new[]
                            {
                                lineNum++.ToString(),
                                line.Product?.Name ?? string.Empty,
                                line.Unit?.Name    ?? string.Empty,
                                line.Quantity.ToString("N2"),
                            });
                        }

                        col.Item().Component(new ItemsTableComponent(columns, rows, fontFamily));

                        if (isDraft)
                            RenderDraftWatermark(col, labels["draftWatermark"], fontFamily);
                    });

                    page.Footer().Component(new FooterComponent(
                        notes: delivery.Notes,
                        postedByUserName: null,
                        postedAt: delivery.PostedAt,
                        lang: lang,
                        fontFamily: fontFamily));
                });
            });
        }

        public async Task<byte[]> GenerateSalesReceiptPdfAsync(int receiptId, string lang, CancellationToken ct = default)
        {
            lang = lang.ToLowerInvariant();
            await _moduleAccess.EnsureSalesEnabledAsync(ct);

            var companyId = _currentUser.CompanyId;
            var company = await LoadCompanyAsync(companyId, ct);
            var receipt = await _receiptRepo.GetByIdWithDetailsAsync(receiptId, companyId, ct)
                            ?? throw BusinessErrors.SalesReceiptNotFound(receiptId);

            return GenerateDocument(lang, doc =>
            {
                var fontFamily = lang == "ar" ? FontArabic : FontDefault;
                var labels = PdfLabels.Get(lang);
                var isDraft = receipt.Status == SalesReceiptStatus.Draft;
                var dateFormat = lang == "ar" ? "dd/MM/yyyy" : "MM/dd/yyyy";

                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(txt => txt.FontFamily(fontFamily));

                    if (lang == "ar")
                    {
                        page.ContentFromRightToLeft();
                    }
                    else
                    {
                        page.ContentFromLeftToRight();
                    }

                    page.Header().Component(new CompanyHeaderComponent(company, lang, fontFamily));

                    page.Content().PaddingTop(8).Column(col =>
                    {
                        col.Item().Component(new DocumentMetaBlock(
                            title: labels["receiptTitle"],
                            docNumber: receipt.ReceiptNumber,
                            docDate: receipt.ReceiptDate,
                            dueDate: null,
                            statusLabel: LocalizeReceiptStatus(receipt.Status, labels),
                            statusColor: GenericStatusColor((int)receipt.Status),
                            customerName: receipt.Customer?.Name ?? "N/A",
                            customerCode: receipt.Customer?.Code ?? "N/A",
                            customerTaxNumber: receipt.Customer?.TaxNumber,
                            lang: lang,
                            fontFamily: fontFamily));

                        // Payment method / reference
                        col.Item().PaddingTop(6).Row(row =>
                        {
                            if (!string.IsNullOrWhiteSpace(receipt.PaymentMethod))
                                row.RelativeItem()
                                    .Text($"{labels["paymentMethod"]}: {receipt.PaymentMethod}")
                                    .FontFamily(fontFamily)
                                    .FontSize(10);

                            if (!string.IsNullOrWhiteSpace(receipt.ReferenceNumber))
                                row.RelativeItem()
                                    .Text($"{labels["reference"]}: {receipt.ReferenceNumber}")
                                    .FontFamily(fontFamily)
                                    .FontSize(10);
                        });

                        // Total amount block
                        col.Item().PaddingTop(6)
                            .Text($"{labels["amount"]}: {receipt.Amount.ToString("N2")}")
                            .FontFamily(fontFamily)
                            .FontSize(13)
                            .Bold();

                        col.Item().PaddingTop(12);

                        // Allocations table
                        var columns = new List<ItemsTableComponent.ColumnDef>
                        {
                            new(labels["no"],          0.5f),
                            new(labels["allocatedTo"], 3.0f),
                            new(labels["amount"],      1.5f, RightAlign: true),
                        };

                        var rows = new List<IReadOnlyList<string>>();
                        int lineNum = 1;
                        foreach (var alloc in receipt.Allocations)
                        {
                            rows.Add(new[]
                            {
                                lineNum++.ToString(),
                                alloc.SalesInvoice?.InvoiceNumber ?? string.Empty,
                                alloc.AllocatedAmount.ToString("N2"),
                            });
                        }

                        col.Item().Component(new ItemsTableComponent(columns, rows, fontFamily));

                        if (isDraft)
                            RenderDraftWatermark(col, labels["draftWatermark"], fontFamily);
                    });

                    page.Footer().Component(new FooterComponent(
                        notes: receipt.Notes,
                        postedByUserName: null,
                        postedAt: receipt.PostedAt,
                        lang: lang,
                        fontFamily: fontFamily));
                });
            });
        }

        public async Task<byte[]> GenerateSalesReturnPdfAsync(int returnId, string lang, CancellationToken ct = default)
        {
            lang = lang.ToLowerInvariant();
            await _moduleAccess.EnsureSalesEnabledAsync(ct);

            var companyId = _currentUser.CompanyId;
            var company = await LoadCompanyAsync(companyId, ct);
            var salesReturn = await _returnRepo.GetByIdWithDetailsAsync(returnId, companyId, ct)
                              ?? throw BusinessErrors.SalesReturnNotFound(returnId);

            return GenerateDocument(lang, doc =>
            {
                var fontFamily = lang == "ar" ? FontArabic : FontDefault;
                var labels = PdfLabels.Get(lang);
                var isDraft = salesReturn.Status == SalesReturnStatus.Draft;

                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(txt => txt.FontFamily(fontFamily));

                    if (lang == "ar")
                    {
                        page.ContentFromRightToLeft();
                    }
                    else
                    {
                        page.ContentFromLeftToRight();
                    }

                    page.Header().Component(new CompanyHeaderComponent(company, lang, fontFamily));

                    page.Content().PaddingTop(8).Column(col =>
                    {
                        col.Item().Component(new DocumentMetaBlock(
                            title: labels["returnTitle"],
                            docNumber: salesReturn.ReturnNumber,
                            docDate: salesReturn.ReturnDate,
                            dueDate: null,
                            statusLabel: LocalizeReturnStatus(salesReturn.Status, labels),
                            statusColor: GenericStatusColor((int)salesReturn.Status),
                            customerName: salesReturn.Customer?.Name ?? "N/A",
                            customerCode: salesReturn.Customer?.Code ?? "N/A",
                            customerTaxNumber: salesReturn.Customer?.TaxNumber, 
                            lang: lang,
                            fontFamily: fontFamily));

                        col.Item().PaddingTop(6).Row(row =>
                        {
                            row.RelativeItem()
                                .Text($"{labels["warehouse"]}: {salesReturn.Warehouse?.Name ?? "N/A"}")
                                .FontFamily(fontFamily)
                                .FontSize(10);

                            if (salesReturn.SalesInvoice is not null)
                                row.RelativeItem()
                                    .Text($"{labels["invoiceNumber"]}: {salesReturn.SalesInvoice.InvoiceNumber}")
                                    .FontFamily(fontFamily)
                                    .FontSize(10);
                        });

                        if (!string.IsNullOrWhiteSpace(salesReturn.Reason))
                            col.Item().PaddingTop(4)
                                .Text($"{labels["reason"]}: {salesReturn.Reason}")
                                .FontFamily(fontFamily)
                                .FontSize(10);

                        col.Item().PaddingTop(10);

                        var columns = new List<ItemsTableComponent.ColumnDef>
                        {
                            new(labels["no"],        0.5f),
                            new(labels["product"],   3.0f),
                            new(labels["unit"],      1.0f),
                            new(labels["quantity"],  1.0f, RightAlign: true),
                            new(labels["unitPrice"], 1.2f, RightAlign: true),
                            new(labels["tax"],       1.0f, RightAlign: true),
                            new(labels["lineTotal"], 1.2f, RightAlign: true),
                        };

                        var rows = new List<IReadOnlyList<string>>();
                        int lineNum = 1;
                        foreach (var line in salesReturn.Lines)
                        {
                            rows.Add(new[]
                            {
                                lineNum++.ToString(),
                                line.Product?.Name ?? string.Empty,
                                line.Unit?.Name    ?? string.Empty,
                                line.Quantity.ToString("N2"),
                                line.UnitPrice.ToString("N2"),
                                line.TaxAmount.ToString("N2"),
                                line.LineTotal.ToString("N2"),
                            });
                        }

                        col.Item().Component(new ItemsTableComponent(columns, rows, fontFamily));

                        col.Item().PaddingTop(12);

                        col.Item().Component(new TotalsBlockComponent(
                            subTotal: salesReturn.SubTotal,
                            discountAmount: 0,
                            taxAmount: salesReturn.TaxAmount,
                            grandTotal: salesReturn.GrandTotal,
                            paidAmount: null,
                            balanceDue: null,
                            lang: lang,
                            fontFamily: fontFamily));

                        if (isDraft)
                            RenderDraftWatermark(col, labels["draftWatermark"], fontFamily);
                    });

                    page.Footer().Component(new FooterComponent(
                        notes: salesReturn.Notes,
                        postedByUserName: null,
                        postedAt: salesReturn.PostedAt,
                        lang: lang,
                        fontFamily: fontFamily));
                });
            });
        }

        // ------------------------------------------------------------------
        // Private helpers
        // ------------------------------------------------------------------

        private async Task<Company> LoadCompanyAsync(int companyId, CancellationToken ct)
        {
            return await _companyRepo.GetByIdAsync(companyId, ct)
                   ?? throw BusinessErrors.CompanyNotFound(companyId);
        }

        /// <summary>Renders the document using QuestPDF and returns a fully in-memory byte array.</summary>
        private static byte[] GenerateDocument(string lang, Action<IDocumentContainer> compose)
        {
            var document = QuestPDF.Fluent.Document.Create(compose);
            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }                                                                               

        /// <summary>Overlays a light diagonal watermark text over the page content column.</summary>
        private static void RenderDraftWatermark(ColumnDescriptor col, string text, string fontFamily)
        {
            col.Item()
                .AlignCenter()
                .AlignMiddle()
                .Rotate(-45)
                .Text(text)
                .FontFamily(fontFamily)
                .FontSize(72)
                .FontColor("#9ca3af")
                .Italic();
        }

        // ---- Enum localisation helpers ----

        private static string LocalizeInvoiceStatus(SalesInvoiceStatus status, Dictionary<string, string> labels)
            => status switch
            {
                SalesInvoiceStatus.Draft => labels["draft"],
                SalesInvoiceStatus.Posted => labels["posted"],
                SalesInvoiceStatus.Cancelled => labels["cancelled"],
                SalesInvoiceStatus.PartiallyDelivered => labels["posted"],
                SalesInvoiceStatus.FullyDelivered => labels["posted"],
                _ => status.ToString(),
            };

        private static string LocalizeDeliveryStatus(SalesDeliveryStatus status, Dictionary<string, string> labels)
            => status switch
            {
                SalesDeliveryStatus.Draft => labels["draft"],
                SalesDeliveryStatus.Posted => labels["posted"],
                SalesDeliveryStatus.Cancelled => labels["cancelled"],
                _ => status.ToString(),
            };

        private static string LocalizeReceiptStatus(SalesReceiptStatus status, Dictionary<string, string> labels)
            => status switch
            {
                SalesReceiptStatus.Draft => labels["draft"],
                SalesReceiptStatus.Posted => labels["posted"],
                SalesReceiptStatus.Cancelled => labels["cancelled"],
                _ => status.ToString(),
            };

        private static string LocalizeReturnStatus(SalesReturnStatus status, Dictionary<string, string> labels)
            => status switch
            {
                SalesReturnStatus.Draft => labels["draft"],
                SalesReturnStatus.Posted => labels["posted"],
                SalesReturnStatus.Cancelled => labels["cancelled"],
                _ => status.ToString(),
            };

        private static string LocalizePaymentStatus(PaymentStatus status, Dictionary<string, string> labels)
            => status switch
            {
                PaymentStatus.Unpaid => labels["unpaid"],
                PaymentStatus.PartiallyPaid => labels["partiallyPaid"],
                PaymentStatus.Paid => labels["paid"],
                _ => status.ToString(),
            };

        // ---- Status colour helpers ----

        private static string InvoiceStatusColor(SalesInvoiceStatus status)
            => status switch
            {
                SalesInvoiceStatus.Draft => StatusOrange,
                SalesInvoiceStatus.Posted => StatusGreen,
                SalesInvoiceStatus.PartiallyDelivered => StatusGreen,
                SalesInvoiceStatus.FullyDelivered => StatusGreen,
                SalesInvoiceStatus.Cancelled => StatusRed,
                _ => StatusOrange,
            };

        private static string PaymentStatusColor(PaymentStatus status)
            => status switch
            {
                PaymentStatus.Paid => StatusGreen,
                PaymentStatus.PartiallyPaid => StatusOrange,
                PaymentStatus.Unpaid => StatusRed,
                _ => StatusOrange,
            };

        /// <summary>
        /// Generic colour for document statuses that share Draft=0, Posted=1, Cancelled=2.
        /// </summary>
        private static string GenericStatusColor(int statusValue)
            => statusValue switch
            {
                0 => StatusOrange,
                1 => StatusGreen,
                2 => StatusRed,
                _ => StatusOrange,
            };
    }
}