using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration
{
    /// <summary>
    /// Bilingual label dictionary for all PDF export documents (Arabic / English).
    /// Always use PdfLabels.Get(lang)[key] inside document components; never hardcode label strings.
    /// </summary>
    public static class PdfLabels
    {
        public static readonly Dictionary<string, string> Arabic = new()
        {
            ["invoiceNumber"] = "رقم الفاتورة",
            ["invoiceDate"] = "تاريخ الفاتورة",
            ["dueDate"] = "تاريخ الاستحقاق",
            ["customer"] = "العميل",
            ["customerCode"] = "كود العميل",
            ["status"] = "الحالة",
            ["paymentStatus"] = "حالة الدفع",
            ["product"] = "المنتج",
            ["unit"] = "الوحدة",
            ["quantity"] = "الكمية",
            ["unitPrice"] = "سعر الوحدة",
            ["discount"] = "الخصم",
            ["tax"] = "الضريبة",
            ["lineTotal"] = "الإجمالي",
            ["subTotal"] = "المجموع الفرعي",
            ["discountAmount"] = "إجمالي الخصم",
            ["taxAmount"] = "إجمالي الضريبة",
            ["grandTotal"] = "الإجمالي الكلي",
            ["paidAmount"] = "المدفوع",
            ["balanceDue"] = "الرصيد المستحق",
            ["notes"] = "ملاحظات",
            ["postedBy"] = "اعتمد بواسطة",
            ["postedAt"] = "تاريخ الاعتماد",
            ["warehouse"] = "المستودع",
            ["deliveryNumber"] = "رقم التسليم",
            ["deliveryDate"] = "تاريخ التسليم",
            ["receiptNumber"] = "رقم الإيصال",
            ["receiptDate"] = "تاريخ الإيصال",
            ["paymentMethod"] = "طريقة الدفع",
            ["reference"] = "المرجع",
            ["amount"] = "المبلغ",
            ["allocatedTo"] = "مخصص للفاتورة",
            ["returnNumber"] = "رقم المرتجع",
            ["returnDate"] = "تاريخ المرتجع",
            ["reason"] = "السبب",
            ["page"] = "صفحة",
            ["of"] = "من",
            ["companyName"] = "اسم الشركة",
            ["taxNumber"] = "الرقم الضريبي",
            ["phone"] = "الهاتف",
            ["address"] = "العنوان",
            ["draft"] = "مسودة",
            ["posted"] = "معتمد",
            ["cancelled"] = "ملغي",
            ["unpaid"] = "غير مدفوع",
            ["partiallyPaid"] = "مدفوع جزئياً",
            ["paid"] = "مدفوع",
            ["invoiceTitle"] = "فاتورة مبيعات",
            ["deliveryTitle"] = "سند تسليم",
            ["receiptTitle"] = "إيصال قبض",
            ["returnTitle"] = "مرتجع مبيعات",
            ["no"] = "#",
            ["draftWatermark"] = "مسودة",
        };

        public static readonly Dictionary<string, string> English = new()
        {
            ["invoiceNumber"] = "Invoice Number",
            ["invoiceDate"] = "Invoice Date",
            ["dueDate"] = "Due Date",
            ["customer"] = "Customer",
            ["customerCode"] = "Customer Code",
            ["status"] = "Status",
            ["paymentStatus"] = "Payment Status",
            ["product"] = "Product",
            ["unit"] = "Unit",
            ["quantity"] = "Quantity",
            ["unitPrice"] = "Unit Price",
            ["discount"] = "Discount",
            ["tax"] = "Tax",
            ["lineTotal"] = "Line Total",
            ["subTotal"] = "Subtotal",
            ["discountAmount"] = "Total Discount",
            ["taxAmount"] = "Total Tax",
            ["grandTotal"] = "Grand Total",
            ["paidAmount"] = "Paid Amount",
            ["balanceDue"] = "Balance Due",
            ["notes"] = "Notes",
            ["postedBy"] = "Posted By",
            ["postedAt"] = "Posted At",
            ["warehouse"] = "Warehouse",
            ["deliveryNumber"] = "Delivery Number",
            ["deliveryDate"] = "Delivery Date",
            ["receiptNumber"] = "Receipt Number",
            ["receiptDate"] = "Receipt Date",
            ["paymentMethod"] = "Payment Method",
            ["reference"] = "Reference",
            ["amount"] = "Amount",
            ["allocatedTo"] = "Allocated To Invoice",
            ["returnNumber"] = "Return Number",
            ["returnDate"] = "Return Date",
            ["reason"] = "Reason",
            ["page"] = "Page",
            ["of"] = "of",
            ["companyName"] = "Company Name",
            ["taxNumber"] = "Tax Number",
            ["phone"] = "Phone",
            ["address"] = "Address",
            ["draft"] = "Draft",
            ["posted"] = "Posted",
            ["cancelled"] = "Cancelled",
            ["unpaid"] = "Unpaid",
            ["partiallyPaid"] = "Partially Paid",
            ["paid"] = "Paid",
            ["invoiceTitle"] = "Sales Invoice",
            ["deliveryTitle"] = "Sales Delivery",
            ["receiptTitle"] = "Sales Receipt",
            ["returnTitle"] = "Sales Return",
            ["no"] = "#",
            ["draftWatermark"] = "DRAFT",
        };

        /// <summary>Returns the label dictionary for the given language code ("ar" or "en").</summary>
        public static Dictionary<string, string> Get(string lang) =>
            lang == "ar" ? Arabic : English;
    }
}