using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Sales
{
    public class SalesDeliveryDto
    {
        public int Id { get; set; }
        public string DeliveryNumber { get; set; } = default!;
        public DateTime DeliveryDate { get; set; }
        public int SalesInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string? Notes { get; set; }
        public List<SalesDeliveryLineDto> Lines { get; set; } = new();
    }

    public class SalesDeliveryLineDto
    {
        public int Id { get; set; }
        public int SalesInvoiceLineId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string ProductCode { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public decimal Quantity { get; set; }
    }

    public class CreateSalesDeliveryRequest
    {
        public int? BranchId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int SalesInvoiceId { get; set; }
        public int WarehouseId { get; set; }
        public string? Notes { get; set; }
        public List<CreateSalesDeliveryLineRequest> Lines { get; set; } = new();
    }

    public class CreateSalesDeliveryLineRequest
    {
        public int SalesInvoiceLineId { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }

    public class SalesDeliveryListDto
    {
        public int Id { get; set; }
        public string DeliveryNumber { get; set; } = default!;
        public DateTime DeliveryDate { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public string CustomerName { get; set; } = default!;
        public string WarehouseName { get; set; } = default!;
        public string Status { get; set; } = default!;
    }

    public class PostDeliveryResponse
    {
        public int DeliveryId { get; set; }
        public string DeliveryNumber { get; set; } = default!;
        public int? InventoryDocumentId { get; set; }
        public string? InventoryDocNumber { get; set; }
    }
}
