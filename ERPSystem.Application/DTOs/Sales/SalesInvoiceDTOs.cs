using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.DTOs.Sales
{
    public class SalesInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string PaymentStatus { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public string? Notes { get; set; }
        public List<SalesInvoiceLineDto> Lines { get; set; } = new();
    }

    public class SalesInvoiceLineDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string ProductCode { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal RemainingQuantity { get; set; }
    }

    public class CreateSalesInvoiceRequest
    {
        public int? BranchId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int CustomerId { get; set; }
        public string? Notes { get; set; }
        public List<CreateSalesInvoiceLineRequest> Lines { get; set; } = new();
    }

    public class CreateSalesInvoiceLineRequest
    {
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateSalesInvoiceRequest
    {
        public DateTime InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int CustomerId { get; set; }
        public string? Notes { get; set; }
        public List<UpdateSalesInvoiceLineRequest> Lines { get; set; } = new();
    }

    public class UpdateSalesInvoiceLineRequest
    {
        public int? Id { get; set; } // null for new lines
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxPercent { get; set; }
        public string? Notes { get; set; }
    }

    public class SalesInvoiceListDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string PaymentStatus { get; set; } = default!;
        public decimal GrandTotal { get; set; }
        public decimal BalanceDue { get; set; }
    }
}
