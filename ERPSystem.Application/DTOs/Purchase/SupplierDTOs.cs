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
    //  Supplier
    // ─────────────────────────────────────────────────────────────

    public class SupplierListDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? TaxNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public int PaymentTermsDays { get; set; }
        public bool IsActive { get; set; }
    }

    public class SupplierDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? TaxNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public int PaymentTermsDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateSupplierDto
    {
        [Required, StringLength(50)]
        public string Code { get; set; } = default!;

        [Required, StringLength(200)]
        public string Name { get; set; } = default!;

        [StringLength(50)]
        public string? TaxNumber { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; }

        [Range(0, 365)]
        public int PaymentTermsDays { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateSupplierDto
    {
        [Required, StringLength(50)]
        public string Code { get; set; } = default!;

        [Required, StringLength(200)]
        public string Name { get; set; } = default!;

        [StringLength(50)]
        public string? TaxNumber { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CreditLimit { get; set; }

        [Range(0, 365)]
        public int PaymentTermsDays { get; set; }

        public bool IsActive { get; set; }
    }


    // ─────────────────────────────────────────────────────────────
    //  Supplier Payment
    // ─────────────────────────────────────────────────────────────

    public class SupplierPaymentListDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = default!;
        public DateOnly PaymentDate { get; set; }
        public string SupplierName { get; set; } = default!;
        public decimal Amount { get; set; }
        public SupplierPaymentStatus Status { get; set; }
    }

    public class SupplierPaymentDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = default!;
        public DateOnly PaymentDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = default!;
        public decimal Amount { get; set; }
        public string? Reference { get; set; }
        public string? Notes { get; set; }
        public SupplierPaymentStatus Status { get; set; }
        public List<SupplierPaymentAllocationDto> Allocations { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SupplierPaymentAllocationDto
    {
        public int Id { get; set; }
        public int PurchaseInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = default!;
        public decimal AllocatedAmount { get; set; }
    }

    public class CreateSupplierPaymentDto
    {
        [Required]
        public int SupplierId { get; set; }

        [Required]
        public DateOnly PaymentDate { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [StringLength(100)]
        public string? Reference { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required, MinLength(1)]
        public List<PaymentAllocationDto> Allocations { get; set; } = new();
    }

    public class PaymentAllocationDto
    {
        [Required]
        public int PurchaseInvoiceId { get; set; }

        [Required, Range(0.01, double.MaxValue)]
        public decimal AllocatedAmount { get; set; }
    }
}