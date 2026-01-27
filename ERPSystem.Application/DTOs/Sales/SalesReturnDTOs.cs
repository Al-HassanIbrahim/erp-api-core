using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Sales
{
    public class SalesReturnDto
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = default!;
        public DateTime ReturnDate { get; set; }
        public int? SalesInvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public List<SalesReturnLineDto> Lines { get; set; } = new();
    }

    public class SalesReturnLineDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string ProductCode { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreateSalesReturnRequest
    {
        public int? BranchId { get; set; }
        public DateTime ReturnDate { get; set; }
        public int? SalesInvoiceId { get; set; }
        public int CustomerId { get; set; }
        public int WarehouseId { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public List<CreateSalesReturnLineRequest> Lines { get; set; } = new();
    }

    public class CreateSalesReturnLineRequest
    {
        public int ProductId { get; set; }
        public int UnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercent { get; set; }
        public string? Notes { get; set; }
    }

    public class SalesReturnListDto
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = default!;
        public DateTime ReturnDate { get; set; }
        public string CustomerName { get; set; } = default!;
        public string WarehouseName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public decimal GrandTotal { get; set; }
    }

    public class PostReturnResponse
    {
        public int ReturnId { get; set; }
        public string ReturnNumber { get; set; } = default!;
        public int? InventoryDocumentId { get; set; }
        public string? InventoryDocNumber { get; set; }
    }
}
