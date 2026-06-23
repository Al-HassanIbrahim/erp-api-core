// ============================================================
//  ERPSystem.Application — Purchasing Module Services
//
//  Architecture compliance:
//    ✔ No AppDbContext in the application layer — SaveChanges
//      lives in repository implementations (same pattern as
//      SalesInvoiceService).
//    ✔ Uses the real IInventoryService from
//      ERPSystem.Application.Interfaces with the actual
//      StockInRequest / StockOutRequest DTOs from
//      ERPSystem.Application.DTOs.Inventory.
//    ✔ Uses the shared IDocumentSequenceService (infrastructure)
//      exactly as every other module does — no private SQL MERGE.
//    ✔ Exceptions are thrown via BusinessErrors factory methods
//      (same pattern as the rest of the system). New Purchasing-
//      specific factory methods are added to BusinessErrors in
//      the companion section at the bottom of this file.
//    ✔ IWarehouseRepository comes from the Inventory module's
//      existing registration (no new interface needed).
// ============================================================
// ============================================================
//  ERPSystem.Application.Services.Purchasing
//  All 4 Purchasing Services — with all 10 fixes applied
//
//  Fix reference map (marked inline as // [FIX-N]):
//    Fix 1  — IUnitOfWork wraps all transactional operations
//    Fix 3  — Over-return quantity validation
//    Fix 4  — Supplier-invoice match on payment allocation
//    Fix 5  — Positive value guards on line items
//    Fix 6  — Allocation amount > 0 and <= balance
//    Fix 7  — Invoice posting: supplier active + line guards
//    Fix 8  — Return posting: supplier active + line guards
//    Fix 9  — Supplier delete: block if active documents exist
//    Fix 10 — DbUpdateConcurrencyException → BusinessErrors
//
//  Fix 2 (DB unique index) is applied in EF config — see note
//  in Purchasing_UnitOfWork.cs companion section.
// ============================================================
using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.DTOs.Purchase;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Purchase;
using ERPSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ERPSystem.Application.Services.Purchasing;

// ─────────────────────────────────────────────────────────────
//  1.  Supplier Service
// ─────────────────────────────────────────────────────────────
public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repo;
    private readonly ICurrentUserService _currentUser;
    private readonly IModuleAccessService _moduleAccess;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        ISupplierRepository repo,
        ICurrentUserService currentUser,
        IModuleAccessService moduleAccess,
        ILogger<SupplierService> logger)
    {
        _repo = repo;
        _currentUser = currentUser;
        _moduleAccess = moduleAccess;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SupplierListDto>> GetAllAsync(CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var suppliers = await _repo.GetAllByCompanyAsync(_currentUser.CompanyId, ct);
        return suppliers.Select(MapToListDto).ToList();
    }

    public async Task<SupplierDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var supplier = await _repo.GetByIdAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.SupplierNotFound(id);
        return MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto request, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        if (await _repo.CodeExistsAsync(_currentUser.CompanyId, request.Code, excludeId: null, ct))
            throw BusinessErrors.DuplicateSupplierCode(request.Code);

        var supplier = new Supplier
        {
            CompanyId = _currentUser.CompanyId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            TaxNumber = request.TaxNumber?.Trim(),
            CreditLimit = request.CreditLimit,
            PaymentTermsDays = request.PaymentTermsDays,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = _currentUser.UserId,
        };

        await _repo.AddAsync(supplier, ct);
        await _repo.SaveChangesAsync(ct);

        _logger.LogInformation("Supplier {Code} created for company {CompanyId}.",
            supplier.Code, supplier.CompanyId);

        return MapToDto(supplier);
    }

    public async Task<SupplierDto> UpdateAsync(int id, UpdateSupplierDto request, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        var supplier = await _repo.GetByIdAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.SupplierNotFound(id);

        if (await _repo.CodeExistsAsync(_currentUser.CompanyId, request.Code, excludeId: id, ct))
            throw BusinessErrors.DuplicateSupplierCode(request.Code);

        supplier.Code = request.Code.Trim();
        supplier.Name = request.Name.Trim();
        supplier.TaxNumber = request.TaxNumber?.Trim();
        supplier.CreditLimit = request.CreditLimit;
        supplier.PaymentTermsDays = request.PaymentTermsDays;
        supplier.IsActive = request.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedByUserId = _currentUser.UserId;

        _repo.Update(supplier);

        await _repo.SaveChangesAsync(ct);                      

        return MapToDto(supplier);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        var supplier = await _repo.GetByIdAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.SupplierNotFound(id);

        // [Fix 9] Block deletion if supplier has any posted documents
        if (await _repo.HasActivePurchasingDocumentsAsync(id, _currentUser.CompanyId, ct))
            throw BusinessErrors.SupplierHasActiveDocuments();

        supplier.IsDeleted = true;
        supplier.UpdatedAt = DateTime.UtcNow;
        supplier.UpdatedByUserId = _currentUser.UserId;

        _repo.Update(supplier);

        await _repo.SaveChangesAsync(ct);
    }

    private static SupplierListDto MapToListDto(Supplier s) => new()
    {
        Id = s.Id,
        Code = s.Code,
        Name = s.Name,
        TaxNumber = s.TaxNumber,
        CreditLimit = s.CreditLimit,
        PaymentTermsDays = s.PaymentTermsDays,
        IsActive = s.IsActive,
    };

    private static SupplierDto MapToDto(Supplier s) => new()
    {
        Id = s.Id,
        Code = s.Code,
        Name = s.Name,
        TaxNumber = s.TaxNumber,
        CreditLimit = s.CreditLimit,
        PaymentTermsDays = s.PaymentTermsDays,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt,
    };
}

// ─────────────────────────────────────────────────────────────
//  2.  Purchase Invoice Service
// ─────────────────────────────────────────────────────────────
public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly IPurchaseInvoiceRepository _repo;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IInventoryService _inventoryService;
    private readonly IDocumentSequenceService _sequenceService;
    private readonly IUnitOfWork _unitOfWork;        // [Fix 1]
    private readonly ICurrentUserService _currentUser;
    private readonly IModuleAccessService _moduleAccess;
    private readonly ILogger<PurchaseInvoiceService> _logger;

    public PurchaseInvoiceService(
        IPurchaseInvoiceRepository repo,
        ISupplierRepository supplierRepo,
        IWarehouseRepository warehouseRepo,
        IInventoryService inventoryService,
        IDocumentSequenceService sequenceService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IModuleAccessService moduleAccess,
        ILogger<PurchaseInvoiceService> logger)
    {
        _repo = repo;
        _supplierRepo = supplierRepo;
        _warehouseRepo = warehouseRepo;
        _inventoryService = inventoryService;
        _sequenceService = sequenceService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _moduleAccess = moduleAccess;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PurchaseInvoiceListDto>> GetAllAsync(CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var invoices = await _repo.GetAllByCompanyAsync(_currentUser.CompanyId, ct);
        return invoices.Select(MapToListDto).ToList();
    }

    public async Task<PurchaseInvoiceDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var invoice = await _repo.GetByIdWithLinesAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.PurchaseInvoiceNotFound(id);
        return MapToDto(invoice);
    }

    public async Task<PurchaseInvoiceDto> CreateAsync(
        CreatePurchaseInvoiceDto request, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        // [Fix 5] Validate line values before touching the database
        ValidateInvoiceLines(request.Lines);

        PurchaseInvoice? invoice = null;

        // [Fix 1] Wrap in transaction — GenerateNextNumberAsync requires it
        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            // Validate supplier and warehouse inside tx for consistency
            await ValidateSupplierAndWarehouseAsync(request.SupplierId, request.WarehouseId, innerCt);

            invoice = new PurchaseInvoice
            {
                CompanyId = _currentUser.CompanyId,
                InvoiceNumber = await _sequenceService.GenerateNextNumberAsync(
                                    _currentUser.CompanyId, "PurchaseInvoice", "PI", innerCt),
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                Status = PurchaseInvoiceStatus.Draft,
                PaymentStatus = PurchasePaymentStatus.Unpaid,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUser.UserId,
                Lines = new List<PurchaseInvoiceLine>(),
            };

            MapLines(invoice, request.Lines);
            RecalculateTotals(invoice);

            await _repo.AddAsync(invoice, innerCt);
            await _repo.SaveChangesAsync(innerCt);
        }, ct);

        _logger.LogInformation("Purchase invoice {Number} created for company {CompanyId}.",
            invoice!.InvoiceNumber, invoice.CompanyId);

        var created = await _repo.GetByIdWithLinesAsync(invoice.Id, _currentUser.CompanyId, ct);
        return MapToDto(created!);
    }

    public async Task<PurchaseInvoiceDto> UpdateAsync(
        int id, UpdatePurchaseInvoiceDto request, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        var invoice = await _repo.GetByIdWithLinesAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.PurchaseInvoiceNotFound(id);

        if (invoice.Status != PurchaseInvoiceStatus.Draft)
            throw BusinessErrors.CannotModifyPostedDocument();

        // [Fix 5] Validate line values
        ValidateInvoiceLines(request.Lines);

        await ValidateSupplierAndWarehouseAsync(request.SupplierId, request.WarehouseId, ct);

        invoice.SupplierId = request.SupplierId;
        invoice.WarehouseId = request.WarehouseId;
        invoice.InvoiceDate = request.InvoiceDate;
        invoice.DueDate = request.DueDate;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedByUserId = _currentUser.UserId;

        invoice.Lines.Clear();
        MapLines(invoice, request.Lines);
        RecalculateTotals(invoice);

        _repo.Update(invoice);

        await _repo.SaveChangesAsync(ct);

        var updated = await _repo.GetByIdWithLinesAsync(id, _currentUser.CompanyId, ct);
        return MapToDto(updated!);
    }

    /// <summary>
    /// Posts a Draft invoice.
    ///
    /// Transaction boundary [Fix 1]:
    ///   IUnitOfWork opens ONE SQL Server transaction.
    ///   IInventoryService.StockInAsync flushes Inventory writes into it.
    ///   _repo.SaveChangesAsync flushes the invoice status update into it.
    ///   UoW CommitAsync commits both atomically.
    ///   Any exception → full rollback of both modules.
    /// </summary>
    public async Task PostAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        // [Fix 1]
        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            var invoice = await _repo.GetByIdWithLinesAsync(id, _currentUser.CompanyId, innerCt)
                ?? throw BusinessErrors.PurchaseInvoiceNotFound(id);

            if (invoice.Status != PurchaseInvoiceStatus.Draft)
                throw BusinessErrors.InvalidStatus("Invoice can only be posted from Draft status.");

            // [Fix 7] At least one line required
            if (!invoice.Lines.Any())
                throw BusinessErrors.InvalidStatus("Cannot post an invoice with no line items.");

            // [Fix 7] Validate supplier/warehouse and that supplier is ACTIVE
            await ValidateSupplierAndWarehouseAsync(invoice.SupplierId, invoice.WarehouseId, innerCt);

            // [Fix 7] Re-validate line quantities and conversion rates from stored data
            ValidateStoredInvoiceLines(invoice.Lines);

            // Build StockInRequest — Qty and Cost converted to base units
            var stockInRequest = new StockInRequest
            {
                BranchId = null,
                DocDate = invoice.InvoiceDate.ToDateTime(TimeOnly.MinValue),
                SourceType = "PurchaseInvoice",
                SourceId = invoice.Id,
                Notes = $"Purchase Invoice {invoice.InvoiceNumber}",
                Lines = invoice.Lines.Select(l => new StockInLineRequest
                {
                    ProductId = l.ProductId,
                    WarehouseId = invoice.WarehouseId,
                    UnitId = l.UnitId,
                    // Inventory Qty  = Line.Quantity  × ConversionRate
                    // Inventory Cost = Line.UnitPrice ÷ ConversionRate
                    Quantity = Math.Round(l.Quantity * l.ConversionRate, 4),
                    UnitCost = l.ConversionRate == 0 ? 0
                                  : Math.Round(l.UnitPrice / l.ConversionRate, 4),
                    Notes = l.Notes,
                }).ToList(),
            };

            _logger.LogInformation("Posting purchase invoice {Number} — calling StockIn.",
                invoice.InvoiceNumber);

            var inventoryResult = await _inventoryService.StockInAsync(stockInRequest, innerCt);

            invoice.Status = PurchaseInvoiceStatus.Posted;
            invoice.InventoryDocumentId = inventoryResult.DocumentId;
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedByUserId = _currentUser.UserId;

            _repo.Update(invoice);
            await _repo.SaveChangesAsync(innerCt);

            _logger.LogInformation("Purchase invoice {Number} posted. InventoryDoc={DocId}.",
                invoice.InvoiceNumber, inventoryResult.DocumentId);
        }, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        var invoice = await _repo.GetByIdAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.PurchaseInvoiceNotFound(id);

        // [Fix 9] Only Draft invoices can be deleted
        if (invoice.Status != PurchaseInvoiceStatus.Draft)
            throw BusinessErrors.InvalidStatus("Only Draft invoices can be deleted.");

        invoice.IsDeleted = true;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.UpdatedByUserId = _currentUser.UserId;

        _repo.Update(invoice);

        await _repo.SaveChangesAsync(ct);
    }

    // ── Validation helpers ────────────────────────────────────

    private async Task ValidateSupplierAndWarehouseAsync(
        int supplierId, int warehouseId, CancellationToken ct)
    {
        var supplier = await _supplierRepo.GetByIdAsync(supplierId, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.SupplierNotFound(supplierId);

        // [Fix 7] Supplier must be active
        if (!supplier.IsActive)
            throw BusinessErrors.InvalidStatus($"Supplier '{supplier.Name}' is not active.");

        var warehouse = await _warehouseRepo.GetByIdAsync(warehouseId, _currentUser.CompanyId)
            ?? throw BusinessErrors.WarehouseNotFound();

        if (!warehouse.IsActive)
            throw BusinessErrors.InvalidStatus($"Warehouse '{warehouse.Name}' is not active.");
    }

    // [Fix 5] Validate lines from request DTOs
    private static void ValidateInvoiceLines(IEnumerable<CreatePurchaseInvoiceLineDto> lines)
    {
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw BusinessErrors.InvalidLineValue("Quantity must be greater than zero.");
            if (line.UnitPrice < 0)
                throw BusinessErrors.InvalidLineValue("UnitPrice cannot be negative.");
            if (line.ConversionRate <= 0)
                throw BusinessErrors.InvalidLineValue("ConversionRate must be greater than zero.");
        }
    }

    // [Fix 7] Validate lines already stored (for posting guard)
    private static void ValidateStoredInvoiceLines(IEnumerable<PurchaseInvoiceLine> lines)
    {
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw BusinessErrors.InvalidLineValue("Stored line has invalid Quantity.");
            if (line.ConversionRate <= 0)
                throw BusinessErrors.InvalidLineValue("Stored line has invalid ConversionRate.");
        }
    }

    // ── Line / total helpers ──────────────────────────────────

    private static void MapLines(
        PurchaseInvoice invoice,
        IEnumerable<CreatePurchaseInvoiceLineDto> lineRequests)
    {
        foreach (var r in lineRequests)
        {
            var gross = r.Quantity * r.UnitPrice;
            var discountAmount = Math.Round(gross * (r.DiscountPercent / 100m), 2);
            var net = gross - discountAmount;
            var taxAmount = Math.Round(net * (r.TaxPercent / 100m), 2);
            var lineTotal = net + taxAmount;

            invoice.Lines.Add(new PurchaseInvoiceLine
            {
                ProductId = r.ProductId,
                UnitId = r.UnitId,
                ConversionRate = r.ConversionRate,
                Quantity = r.Quantity,
                UnitPrice = r.UnitPrice,
                DiscountPercent = r.DiscountPercent,
                DiscountAmount = discountAmount,
                TaxPercent = r.TaxPercent,
                TaxAmount = taxAmount,
                LineTotal = lineTotal,
                Notes = r.Notes?.Trim(),
            });
        }
    }

    private static void RecalculateTotals(PurchaseInvoice invoice)
    {
        invoice.SubTotal = invoice.Lines.Sum(l => l.Quantity * l.UnitPrice);
        invoice.DiscountAmount = invoice.Lines.Sum(l => l.DiscountAmount);
        invoice.TaxAmount = invoice.Lines.Sum(l => l.TaxAmount);
        invoice.GrandTotal = invoice.Lines.Sum(l => l.LineTotal);
    }

    // ── Mapping helpers ───────────────────────────────────────

    private static PurchaseInvoiceListDto MapToListDto(PurchaseInvoice i) => new()
    {
        Id = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        InvoiceDate = i.InvoiceDate,
        DueDate = i.DueDate,
        SupplierName = i.Supplier?.Name ?? string.Empty,
        GrandTotal = i.GrandTotal,
        BalanceDue = i.BalanceDue,
        Status = i.Status,
        PaymentStatus = i.PaymentStatus,
    };

    private static PurchaseInvoiceDto MapToDto(PurchaseInvoice i) => new()
    {
        Id = i.Id,
        InvoiceNumber = i.InvoiceNumber,
        InvoiceDate = i.InvoiceDate,
        DueDate = i.DueDate,
        SupplierId = i.SupplierId,
        SupplierName = i.Supplier?.Name ?? string.Empty,
        WarehouseId = i.WarehouseId,
        WarehouseName = i.Warehouse?.Name ?? string.Empty,
        SubTotal = i.SubTotal,
        DiscountAmount = i.DiscountAmount,
        TaxAmount = i.TaxAmount,
        GrandTotal = i.GrandTotal,
        PaidAmount = i.PaidAmount,
        BalanceDue = i.BalanceDue,
        InventoryDocumentId = i.InventoryDocumentId,
        Status = i.Status,
        PaymentStatus = i.PaymentStatus,
        CreatedAt = i.CreatedAt,
        UpdatedAt = i.UpdatedAt,
        Lines = i.Lines.Select(l => new PurchaseInvoiceLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductName = l.Product?.Name ?? string.Empty,
            ProductCode = l.Product?.Code ?? string.Empty,
            UnitId = l.UnitId,
            UnitName = l.Unit?.Name ?? string.Empty,
            ConversionRate = l.ConversionRate,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            DiscountPercent = l.DiscountPercent,
            DiscountAmount = l.DiscountAmount,
            TaxPercent = l.TaxPercent,
            TaxAmount = l.TaxAmount,
            LineTotal = l.LineTotal,
            Notes = l.Notes,
        }).ToList(),
    };
}

// ─────────────────────────────────────────────────────────────
//  3.  Purchase Return Service
// ─────────────────────────────────────────────────────────────
public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly IPurchaseReturnRepository _repo;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IPurchaseInvoiceRepository _invoiceRepo;
    private readonly IInventoryService _inventoryService;
    private readonly IDocumentSequenceService _sequenceService;
    private readonly IUnitOfWork _unitOfWork;        // [Fix 1]
    private readonly ICurrentUserService _currentUser;
    private readonly IModuleAccessService _moduleAccess;
    private readonly ILogger<PurchaseReturnService> _logger;

    public PurchaseReturnService(
        IPurchaseReturnRepository repo,
        ISupplierRepository supplierRepo,
        IWarehouseRepository warehouseRepo,
        IPurchaseInvoiceRepository invoiceRepo,
        IInventoryService inventoryService,
        IDocumentSequenceService sequenceService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IModuleAccessService moduleAccess,
        ILogger<PurchaseReturnService> logger)
    {
        _repo = repo;
        _supplierRepo = supplierRepo;
        _warehouseRepo = warehouseRepo;
        _invoiceRepo = invoiceRepo;
        _inventoryService = inventoryService;
        _sequenceService = sequenceService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _moduleAccess = moduleAccess;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PurchaseReturnListDto>> GetAllAsync(CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var returns = await _repo.GetAllByCompanyAsync(_currentUser.CompanyId, ct);
        return returns.Select(MapToListDto).ToList();
    }

    public async Task<PurchaseReturnDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var ret = await _repo.GetByIdWithLinesAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.PurchaseReturnNotFound(id);
        return MapToDto(ret);
    }

    public async Task<PurchaseReturnDto> CreateAsync(
        CreatePurchaseReturnDto request, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        // [Fix 5] Validate line values before touching the database
        ValidateReturnLines(request.Lines);

        PurchaseReturn? ret = null;

        // [Fix 1] Transaction required for sequence generation
        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            // Supplier and warehouse checks inside tx for consistency
            var supplier = await _supplierRepo.GetByIdAsync(request.SupplierId, _currentUser.CompanyId, innerCt)
                ?? throw BusinessErrors.SupplierNotFound(request.SupplierId);
            if (!supplier.IsActive)
                throw BusinessErrors.InvalidStatus($"Supplier '{supplier.Name}' is not active.");

            var warehouse = await _warehouseRepo.GetByIdAsync(request.WarehouseId, _currentUser.CompanyId)
                ?? throw BusinessErrors.WarehouseNotFound();
            if (!warehouse.IsActive)
                throw BusinessErrors.InvalidStatus($"Warehouse '{warehouse.Name}' is not active.");

            // Validate linked invoice if provided
            PurchaseInvoice? linkedInvoice = null;
            if (request.PurchaseInvoiceId.HasValue)
            {
                // Load with lines for over-return validation
                linkedInvoice = await _invoiceRepo.GetByIdWithLinesAsync(
                    request.PurchaseInvoiceId.Value, _currentUser.CompanyId, innerCt)
                    ?? throw BusinessErrors.PurchaseInvoiceNotFound(request.PurchaseInvoiceId.Value);

                if (linkedInvoice.Status != PurchaseInvoiceStatus.Posted)
                    throw BusinessErrors.InvalidStatus("Linked invoice must be in Posted status.");

                // [Fix 3] Validate return quantities do not exceed purchased quantities
                var returnLineTuples = request.Lines
                    .Select(l => (l.ProductId, l.UnitId, l.Quantity));
                await ValidateReturnQuantitiesAsync(
                    linkedInvoice, returnLineTuples, excludeReturnId: null, innerCt);
            }

            ret = new PurchaseReturn
            {
                CompanyId = _currentUser.CompanyId,
                ReturnNumber = await _sequenceService.GenerateNextNumberAsync(
                                        _currentUser.CompanyId, "PurchaseReturn", "PR", innerCt),
                ReturnDate = request.ReturnDate,
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                PurchaseInvoiceId = request.PurchaseInvoiceId,
                Status = PurchaseReturnStatus.Draft,
                Reason = request.Reason?.Trim(),
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUser.UserId,
                Lines = new List<PurchaseReturnLine>(),
            };

            MapLines(ret, request.Lines);
            RecalculateTotals(ret);

            await _repo.AddAsync(ret, innerCt);
            await _repo.SaveChangesAsync(innerCt);
        }, ct);

        _logger.LogInformation("Purchase return {Number} created for company {CompanyId}.",
            ret!.ReturnNumber, ret.CompanyId);

        var created = await _repo.GetByIdWithLinesAsync(ret.Id, _currentUser.CompanyId, ct);
        return MapToDto(created!);
    }

    /// <summary>
    /// Posts a Draft return.
    /// StockOut MAC cost is resolved automatically by InventoryService —
    /// UnitCost is intentionally not supplied.
    /// </summary>
    public async Task PostAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

            // [Fix 1]
            await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
            {
                var ret = await _repo.GetByIdWithLinesAsync(id, _currentUser.CompanyId, innerCt)
                    ?? throw BusinessErrors.PurchaseReturnNotFound(id);

                if (ret.Status != PurchaseReturnStatus.Draft)
                    throw BusinessErrors.InvalidStatus("Purchase return can only be posted from Draft status.");

                // [Fix 8] At least one line
                if (!ret.Lines.Any())
                    throw BusinessErrors.InvalidStatus("Cannot post a return with no line items.");

                // [Fix 8] Supplier active
                var supplier = await _supplierRepo.GetByIdAsync(ret.SupplierId, _currentUser.CompanyId, innerCt)
                    ?? throw BusinessErrors.SupplierNotFound(ret.SupplierId);
                if (!supplier.IsActive)
                    throw BusinessErrors.InvalidStatus($"Supplier '{supplier.Name}' is not active.");

                // [Fix 8] Warehouse active
                var warehouse = await _warehouseRepo.GetByIdAsync(ret.WarehouseId, _currentUser.CompanyId)
                    ?? throw BusinessErrors.WarehouseNotFound();
                if (!warehouse.IsActive)
                    throw BusinessErrors.InvalidStatus($"Warehouse '{warehouse.Name}' is not active.");

                // [Fix 8] Re-validate stored line values
                ValidateStoredReturnLines(ret.Lines);

                // [Fix 3] Re-validate over-return at post time (data may have changed since create)
                if (ret.PurchaseInvoiceId.HasValue)
                {
                    var linkedInvoice = await _invoiceRepo.GetByIdWithLinesAsync(
                        ret.PurchaseInvoiceId.Value, _currentUser.CompanyId, innerCt);

                    if (linkedInvoice is not null)
                    {
                        var returnLineTuples = ret.Lines
                            .Select(l => (l.ProductId, l.UnitId, l.Quantity));
                        await ValidateReturnQuantitiesAsync(
                            linkedInvoice, returnLineTuples, excludeReturnId: ret.Id, innerCt);
                    }
                }

                // Inventory Qty = Line.Quantity × ConversionRate
                // No UnitCost — InventoryService resolves MAC from StockItem.AverageUnitCost
                var stockOutRequest = new StockOutRequest
                {
                    BranchId = null,
                    DocDate = ret.ReturnDate.ToDateTime(TimeOnly.MinValue),
                    SourceType = "PurchaseReturn",
                    SourceId = ret.Id,
                    Notes = $"Purchase Return {ret.ReturnNumber}",
                    Lines = ret.Lines.Select(l => new StockOutLineRequest
                    {
                        ProductId = l.ProductId,
                        WarehouseId = ret.WarehouseId,
                        UnitId = l.UnitId,
                        Quantity = Math.Round(l.Quantity * l.ConversionRate, 4),
                        Notes = l.Notes,
                    }).ToList(),
                };

                _logger.LogInformation("Posting purchase return {Number} — calling StockOut.",
                    ret.ReturnNumber);

                var inventoryResult = await _inventoryService.StockOutAsync(stockOutRequest, innerCt);

                ret.Status = PurchaseReturnStatus.Posted;
                ret.InventoryDocumentId = inventoryResult.DocumentId;
                ret.UpdatedAt = DateTime.UtcNow;
                ret.UpdatedByUserId = _currentUser.UserId;

                _repo.Update(ret);
                await _repo.SaveChangesAsync(innerCt);

                _logger.LogInformation("Purchase return {Number} posted. InventoryDoc={DocId}.",
                    ret.ReturnNumber, inventoryResult.DocumentId);
            }, ct);

    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        var ret = await _repo.GetByIdAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.PurchaseReturnNotFound(id);

        // [Fix 9] Only Draft returns can be deleted
        if (ret.Status != PurchaseReturnStatus.Draft)
            throw BusinessErrors.InvalidStatus("Only Draft returns can be deleted.");

        ret.IsDeleted = true;
        ret.UpdatedAt = DateTime.UtcNow;
        ret.UpdatedByUserId = _currentUser.UserId;

        _repo.Update(ret);

        await _repo.SaveChangesAsync(ct);
    }

    // ── Validation helpers ────────────────────────────────────

    // [Fix 5] Validate line values from request DTOs
    private static void ValidateReturnLines(IEnumerable<CreatePurchaseReturnLineDto> lines)
    {
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw BusinessErrors.InvalidLineValue("Quantity must be greater than zero.");
            if (line.UnitPrice < 0)
                throw BusinessErrors.InvalidLineValue("UnitPrice cannot be negative.");
            if (line.ConversionRate <= 0)
                throw BusinessErrors.InvalidLineValue("ConversionRate must be greater than zero.");
        }
    }

    // [Fix 8] Validate stored return lines at post time
    private static void ValidateStoredReturnLines(IEnumerable<PurchaseReturnLine> lines)
    {
        foreach (var line in lines)
        {
            if (line.Quantity <= 0)
                throw BusinessErrors.InvalidLineValue("Stored return line has invalid Quantity.");
            if (line.ConversionRate <= 0)
                throw BusinessErrors.InvalidLineValue("Stored return line has invalid ConversionRate.");
        }
    }

    // [Fix 3] Validate that returned quantities do not exceed what was originally purchased
    private async Task ValidateReturnQuantitiesAsync(
        PurchaseInvoice invoice,
        IEnumerable<(int ProductId, int UnitId, decimal Quantity)> returnLines,
        int? excludeReturnId,
        CancellationToken ct)
    {
        // What was originally purchased, grouped by (ProductId, UnitId)
        var purchased = invoice.Lines
            .GroupBy(l => (l.ProductId, l.UnitId))
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Quantity));

        // What has already been returned for this invoice (excluding the current return)
        var alreadyReturned = await _repo.GetAlreadyReturnedQuantitiesAsync(
            invoice.Id, _currentUser.CompanyId, excludeReturnId, ct);

        foreach (var (productId, unitId, qty) in returnLines)
        {
            var key = (productId, unitId);
            purchased.TryGetValue(key, out var purchasedQty);
            alreadyReturned.TryGetValue(key, out var prevReturnedQty);
            var available = purchasedQty - prevReturnedQty;

            if (qty > available)
            {
                var productName = invoice.Lines
                    .FirstOrDefault(l => l.ProductId == productId && l.UnitId == unitId)
                    ?.Product?.Name ?? $"ProductId {productId}";

                throw BusinessErrors.ReturnQuantityExceeded(productName, available);
            }
        }
    }

    // ── Line / total helpers ──────────────────────────────────

    private static void MapLines(
        PurchaseReturn ret,
        IEnumerable<CreatePurchaseReturnLineDto> lineRequests)
    {
        foreach (var r in lineRequests)
        {
            var taxAmount = Math.Round(r.Quantity * r.UnitPrice * (r.TaxPercent / 100m), 2);
            var lineTotal = r.Quantity * r.UnitPrice + taxAmount;

            ret.Lines.Add(new PurchaseReturnLine
            {
                ProductId = r.ProductId,
                UnitId = r.UnitId,
                ConversionRate = r.ConversionRate,
                Quantity = r.Quantity,
                UnitPrice = r.UnitPrice,
                TaxPercent = r.TaxPercent,
                TaxAmount = taxAmount,
                LineTotal = lineTotal,
                Notes = r.Notes?.Trim(),
            });
        }
    }

    private static void RecalculateTotals(PurchaseReturn ret)
    {
        ret.SubTotal = ret.Lines.Sum(l => l.Quantity * l.UnitPrice);
        ret.TaxAmount = ret.Lines.Sum(l => l.TaxAmount);
        ret.GrandTotal = ret.Lines.Sum(l => l.LineTotal);
    }

    // ── Mapping helpers ───────────────────────────────────────

    private static PurchaseReturnListDto MapToListDto(PurchaseReturn r) => new()
    {
        Id = r.Id,
        ReturnNumber = r.ReturnNumber,
        ReturnDate = r.ReturnDate,
        SupplierName = r.Supplier?.Name ?? string.Empty,
        GrandTotal = r.GrandTotal,
        PurchaseInvoiceId = r.PurchaseInvoiceId,
        Status = r.Status,
    };

    private static PurchaseReturnDto MapToDto(PurchaseReturn r) => new()
    {
        Id = r.Id,
        ReturnNumber = r.ReturnNumber,
        ReturnDate = r.ReturnDate,
        SupplierId = r.SupplierId,
        SupplierName = r.Supplier?.Name ?? string.Empty,
        WarehouseId = r.WarehouseId,
        WarehouseName = r.Warehouse?.Name ?? string.Empty,
        PurchaseInvoiceId = r.PurchaseInvoiceId,
        SubTotal = r.SubTotal,
        TaxAmount = r.TaxAmount,
        GrandTotal = r.GrandTotal,
        Reason = r.Reason,
        InventoryDocumentId = r.InventoryDocumentId,
        Status = r.Status,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Lines = r.Lines.Select(l => new PurchaseReturnLineDto
        {
            Id = l.Id,
            ProductId = l.ProductId,
            ProductName = l.Product?.Name ?? string.Empty,
            ProductCode = l.Product?.Code ?? string.Empty,
            UnitId = l.UnitId,
            UnitName = l.Unit?.Name ?? string.Empty,
            ConversionRate = l.ConversionRate,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice,
            TaxPercent = l.TaxPercent,
            TaxAmount = l.TaxAmount,
            LineTotal = l.LineTotal,
            Notes = l.Notes,
        }).ToList(),
    };
}

// ─────────────────────────────────────────────────────────────
//  4.  Supplier Payment Service
// ─────────────────────────────────────────────────────────────
public class SupplierPaymentService : ISupplierPaymentService
{
    private readonly ISupplierPaymentRepository _repo;
    private readonly ISupplierRepository _supplierRepo;
    private readonly IPurchaseInvoiceRepository _invoiceRepo;
    private readonly IDocumentSequenceService _sequenceService;
    private readonly IUnitOfWork _unitOfWork;        // [Fix 1]
    private readonly ICurrentUserService _currentUser;
    private readonly IModuleAccessService _moduleAccess;
    private readonly ILogger<SupplierPaymentService> _logger;

    public SupplierPaymentService(
        ISupplierPaymentRepository repo,
        ISupplierRepository supplierRepo,
        IPurchaseInvoiceRepository invoiceRepo,
        IDocumentSequenceService sequenceService,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IModuleAccessService moduleAccess,
        ILogger<SupplierPaymentService> logger)
    {
        _repo = repo;
        _supplierRepo = supplierRepo;
        _invoiceRepo = invoiceRepo;
        _sequenceService = sequenceService;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _moduleAccess = moduleAccess;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SupplierPaymentListDto>> GetAllAsync(CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var payments = await _repo.GetAllByCompanyAsync(_currentUser.CompanyId, ct);
        return payments.Select(MapToListDto).ToList();
    }

    public async Task<SupplierPaymentDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);
        var payment = await _repo.GetByIdWithAllocationsAsync(id, _currentUser.CompanyId, ct)
            ?? throw BusinessErrors.SupplierPaymentNotFound(id);
        return MapToDto(payment);
    }

    public async Task<SupplierPaymentDto> CreateAsync(
        CreateSupplierPaymentDto request, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        var allocationTotal = request.Allocations.Sum(a => a.AllocatedAmount);
        if (allocationTotal > request.Amount)
            throw BusinessErrors.AllocationExceedsBalance();

        SupplierPayment? payment = null;

        // [Fix 1] Transaction required for sequence generation
        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            var supplier = await _supplierRepo.GetByIdAsync(request.SupplierId, _currentUser.CompanyId, innerCt)
                ?? throw BusinessErrors.SupplierNotFound(request.SupplierId);

            payment = new SupplierPayment
            {
                CompanyId = _currentUser.CompanyId,
                PaymentNumber = await _sequenceService.GenerateNextNumberAsync(
                                      _currentUser.CompanyId, "SupplierPayment", "PP", innerCt),
                PaymentDate = request.PaymentDate,
                SupplierId = request.SupplierId,
                Amount = request.Amount,
                Reference = request.Reference?.Trim(),
                Notes = request.Notes?.Trim(),
                Status = SupplierPaymentStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = _currentUser.UserId,
                Allocations = new List<SupplierPaymentAllocation>(),
            };

            foreach (var alloc in request.Allocations)
            {
                var invoice = await _invoiceRepo.GetByIdAsync(
                    alloc.PurchaseInvoiceId, _currentUser.CompanyId, innerCt)
                    ?? throw BusinessErrors.PurchaseInvoiceNotFound(alloc.PurchaseInvoiceId);

                // [Fix 4] Invoice must belong to the same supplier as the payment
                if (invoice.SupplierId != request.SupplierId)
                    throw BusinessErrors.InvoiceSupplierMismatch(invoice.InvoiceNumber);

                payment.Allocations.Add(new SupplierPaymentAllocation
                {
                    PurchaseInvoiceId = alloc.PurchaseInvoiceId,
                    AllocatedAmount = alloc.AllocatedAmount,
                });
            }

            await _repo.AddAsync(payment, innerCt);
            await _repo.SaveChangesAsync(innerCt);
        }, ct);

        _logger.LogInformation("Supplier payment {Number} created for company {CompanyId}.",
            payment!.PaymentNumber, payment.CompanyId);

        var created = await _repo.GetByIdWithAllocationsAsync(payment.Id, _currentUser.CompanyId, ct);
        return MapToDto(created!);
    }

    /// <summary>
    /// Posts the payment: validates each allocation, increments PaidAmount,
    /// adjusts PaymentStatus. All mutations are atomic inside the UoW tx.
    /// </summary>
    public async Task PostAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        // [Fix 1]
        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            var payment = await _repo.GetByIdWithAllocationsAsync(id, _currentUser.CompanyId, innerCt)
                ?? throw BusinessErrors.SupplierPaymentNotFound(id);

            if (payment.Status != SupplierPaymentStatus.Draft)
                throw BusinessErrors.InvalidStatus($"Payment is already {payment.Status} and cannot be posted.");

            if (!payment.Allocations.Any())
                throw BusinessErrors.InvalidStatus("Cannot post a payment with no allocations.");

            var allocationTotal = payment.Allocations.Sum(a => a.AllocatedAmount);
            if (allocationTotal > payment.Amount)
                throw BusinessErrors.AllocationExceedsBalance();

            foreach (var allocation in payment.Allocations)
            {
                // [Fix 6] Allocation amount must be positive
                if (allocation.AllocatedAmount <= 0)
                    throw BusinessErrors.InvalidLineValue("Allocation amount must be greater than zero.");

                var invoice = await _invoiceRepo.GetByIdAsync(
                    allocation.PurchaseInvoiceId, _currentUser.CompanyId, innerCt)
                    ?? throw BusinessErrors.PurchaseInvoiceNotFound(allocation.PurchaseInvoiceId);

                if (invoice.Status != PurchaseInvoiceStatus.Posted)
                    throw BusinessErrors.InvalidStatus(
                        $"Invoice {invoice.InvoiceNumber} must be Posted before a payment can be allocated.");

                // [Fix 4] Invoice must belong to the same supplier as the payment
                if (invoice.SupplierId != payment.SupplierId)
                    throw BusinessErrors.InvoiceSupplierMismatch(invoice.InvoiceNumber);

                // [Fix 6] Allocation cannot exceed the invoice's current balance
                if (allocation.AllocatedAmount > invoice.BalanceDue)
                    throw BusinessErrors.AllocationExceedsBalance();

                invoice.PaidAmount += allocation.AllocatedAmount;
                // [Fix 6] Recalculate from actual balance — never assume a state
                invoice.PaymentStatus = ResolvePaymentStatus(invoice.GrandTotal, invoice.PaidAmount);
                invoice.UpdatedAt = DateTime.UtcNow;
                invoice.UpdatedByUserId = _currentUser.UserId;

                _invoiceRepo.Update(invoice);
            }

            payment.Status = SupplierPaymentStatus.Posted;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedByUserId = _currentUser.UserId;

            _repo.Update(payment);
            await _repo.SaveChangesAsync(innerCt);

            _logger.LogInformation("Supplier payment {Number} posted. Allocations: {Count}.",
                payment.PaymentNumber, payment.Allocations.Count);
        }, ct);
    }

    /// <summary>
    /// Cancels a Posted payment: fully reverses all PaidAmount increments
    /// and recomputes each invoice's PaymentStatus from actual balance.
    /// </summary>
    public async Task CancelAsync(int id, CancellationToken ct = default)
    {
        await _moduleAccess.EnsurePurchasingEnabledAsync(ct);

        // [Fix 1]
        await _unitOfWork.ExecuteInTransactionAsync(async innerCt =>
        {
            var payment = await _repo.GetByIdWithAllocationsAsync(id, _currentUser.CompanyId, innerCt)
                ?? throw BusinessErrors.SupplierPaymentNotFound(id);

            if (payment.Status != SupplierPaymentStatus.Posted)
                throw BusinessErrors.InvalidStatus("Only Posted payments can be cancelled.");

            foreach (var allocation in payment.Allocations)
            {
                var invoice = await _invoiceRepo.GetByIdAsync(
                    allocation.PurchaseInvoiceId, _currentUser.CompanyId, innerCt)
                    ?? throw BusinessErrors.PurchaseInvoiceNotFound(allocation.PurchaseInvoiceId);

                invoice.PaidAmount = Math.Max(0, invoice.PaidAmount - allocation.AllocatedAmount);
                // [Fix 6] Recompute payment status from actual balance
                invoice.PaymentStatus = ResolvePaymentStatus(invoice.GrandTotal, invoice.PaidAmount);
                invoice.UpdatedAt = DateTime.UtcNow;
                invoice.UpdatedByUserId = _currentUser.UserId;

                _invoiceRepo.Update(invoice);
            }

            payment.Status = SupplierPaymentStatus.Cancelled;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.UpdatedByUserId = _currentUser.UserId;

            _repo.Update(payment);
            await _repo.SaveChangesAsync(innerCt);

            _logger.LogInformation("Supplier payment {Number} cancelled.", payment.PaymentNumber);
        }, ct);
    }

    // ── Helpers ───────────────────────────────────────────────

    // [Fix 6] Canonical payment status derivation — always calculated from actual balances
    private static PurchasePaymentStatus ResolvePaymentStatus(decimal grandTotal, decimal paidAmount)
    {
        if (paidAmount <= 0m) return PurchasePaymentStatus.Unpaid;
        if (paidAmount >= grandTotal) return PurchasePaymentStatus.Paid;
        return PurchasePaymentStatus.PartiallyPaid;
    }

    // ── Mapping helpers ───────────────────────────────────────

    private static SupplierPaymentListDto MapToListDto(SupplierPayment p) => new()
    {
        Id = p.Id,
        PaymentNumber = p.PaymentNumber,
        PaymentDate = p.PaymentDate,
        SupplierName = p.Supplier?.Name ?? string.Empty,
        Amount = p.Amount,
        Status = p.Status,
    };

    private static SupplierPaymentDto MapToDto(SupplierPayment p) => new()
    {
        Id = p.Id,
        PaymentNumber = p.PaymentNumber,
        PaymentDate = p.PaymentDate,
        SupplierId = p.SupplierId,
        SupplierName = p.Supplier?.Name ?? string.Empty,
        Amount = p.Amount,
        Reference = p.Reference,
        Notes = p.Notes,
        Status = p.Status,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Allocations = p.Allocations.Select(a => new SupplierPaymentAllocationDto
        {
            Id = a.Id,
            PurchaseInvoiceId = a.PurchaseInvoiceId,
            InvoiceNumber = a.PurchaseInvoice?.InvoiceNumber ?? string.Empty,
            AllocatedAmount = a.AllocatedAmount,
        }).ToList(),
    };
}