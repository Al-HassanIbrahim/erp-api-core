using ERPSystem.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Purchase
{
    // ─────────────────────────────────────────────────────────────
    //  Purchase Invoice
    // ─────────────────────────────────────────────────────────────

    public class PurchaseInvoiceListDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public DateOnly InvoiceDate { get; set; }
        public DateOnly DueDate { get; set; }
        public string SupplierName { get; set; } = default!;
        public decimal GrandTotal { get; set; }
        public decimal BalanceDue { get; set; }
        public PurchaseInvoiceStatus Status { get; set; }
        public PurchasePaymentStatus PaymentStatus { get; set; }
    }

    public class PurchaseInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public DateOnly InvoiceDate { get; set; }
        public DateOnly DueDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = default!;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = default!;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceDue { get; set; }
        public int? InventoryDocumentId { get; set; }
        public PurchaseInvoiceStatus Status { get; set; }
        public PurchasePaymentStatus PaymentStatus { get; set; }
        public List<PurchaseInvoiceLineDto> Lines { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PurchaseInvoiceLineDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string ProductCode { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public decimal ConversionRate { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }

    public class CreatePurchaseInvoiceDto
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public DateOnly InvoiceDate { get; set; }

        [Required]
        public DateOnly DueDate { get; set; }

        [Required, MinLength(1)]
        public List<CreatePurchaseInvoiceLineDto> Lines { get; set; } = new();
    }

    public class CreatePurchaseInvoiceLineDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UnitId { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal ConversionRate { get; set; } = 1m;

        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Range(0, 100)]
        public decimal TaxPercent { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    // PUT reuses the same line shape as POST
    public class UpdatePurchaseInvoiceDto
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public DateOnly InvoiceDate { get; set; }

        [Required]
        public DateOnly DueDate { get; set; }

        [Required, MinLength(1)]
        public List<CreatePurchaseInvoiceLineDto> Lines { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────────
    //  Purchase Return
    // ─────────────────────────────────────────────────────────────

    public class PurchaseReturnListDto
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = default!;
        public DateOnly ReturnDate { get; set; }
        public string SupplierName { get; set; } = default!;
        public decimal GrandTotal { get; set; }
        public int? PurchaseInvoiceId { get; set; }
        public PurchaseReturnStatus Status { get; set; }
    }

    public class PurchaseReturnDto
    {
        public int Id { get; set; }
        public string ReturnNumber { get; set; } = default!;
        public DateOnly ReturnDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = default!;
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = default!;
        public int? PurchaseInvoiceId { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Reason { get; set; }
        public int? InventoryDocumentId { get; set; }
        public PurchaseReturnStatus Status { get; set; }
        public List<PurchaseReturnLineDto> Lines { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class PurchaseReturnLineDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = default!;
        public string ProductCode { get; set; } = default!;
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public decimal ConversionRate { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxPercent { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotal { get; set; }
        public string? Notes { get; set; }
    }

    public class CreatePurchaseReturnDto
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        public DateOnly ReturnDate { get; set; }

        public int? PurchaseInvoiceId { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }

        [Required, MinLength(1)]
        public List<CreatePurchaseReturnLineDto> Lines { get; set; } = new();
    }

    public class CreatePurchaseReturnLineDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UnitId { get; set; }

        [Range(0.0001, double.MaxValue)]
        public decimal ConversionRate { get; set; } = 1m;

        [Range(0.0001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Range(0, 100)]
        public decimal TaxPercent { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
