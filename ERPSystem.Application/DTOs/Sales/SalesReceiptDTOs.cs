using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Sales
{
    public class SalesReceiptDto
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; } = default!;
        public DateTime ReceiptDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string Status { get; set; } = default!;
        public string? Notes { get; set; }
        public List<SalesReceiptAllocationDto> Allocations { get; set; } = new();
    }

    public class SalesReceiptAllocationDto
    {
        public int Id { get; set; }
        public int SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public decimal AllocatedAmount { get; set; }
    }

    public class CreateSalesReceiptRequest
    {
        public int? BranchId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public List<CreateReceiptAllocationRequest> Allocations { get; set; } = new();
    }

    public class CreateReceiptAllocationRequest
    {
        public int SalesInvoiceId { get; set; }
        public decimal AllocatedAmount { get; set; }
    }

    public class SalesReceiptListDto
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; } = default!;
        public DateTime ReceiptDate { get; set; }
        public string CustomerName { get; set; } = default!;
        public decimal Amount { get; set; }
        public string Status { get; set; } = default!;
    }
}
