using ERPSystem.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.Application.Interfaces
{
    public interface IPdfExportService
    {
        Task<byte[]> GenerateSalesInvoicePdfAsync(int invoiceId, string lang, CancellationToken ct = default);
        //Task<byte[]> GenerateSalesInvoicesBulkPdfAsync(int? customerId,SalesInvoiceStatus? status,DateTime? fromDate,DateTime? toDate,string lang,CancellationToken ct);
        Task<byte[]> GenerateSalesDeliveryPdfAsync(int deliveryId, string lang, CancellationToken ct = default);
        Task<byte[]> GenerateSalesReceiptPdfAsync(int receiptId, string lang, CancellationToken ct = default);
        Task<byte[]> GenerateSalesReturnPdfAsync(int returnId, string lang, CancellationToken ct = default);
    }
}   