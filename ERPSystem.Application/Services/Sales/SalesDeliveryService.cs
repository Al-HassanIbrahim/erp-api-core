using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Services.Sales
{
    public class SalesDeliveryService : ISalesDeliveryService
    {
        private readonly ISalesDeliveryRepository _repository;
        private readonly ISalesInvoiceRepository _invoiceRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;
        private readonly IInventoryService _inventoryService;

        public SalesDeliveryService(
            ISalesDeliveryRepository repository,
            ISalesInvoiceRepository invoiceRepository,
            IWarehouseRepository warehouseRepository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess,
            IInventoryService inventoryService)
        {
            _repository = repository;
            _invoiceRepository = invoiceRepository;
            _warehouseRepository = warehouseRepository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
            _inventoryService = inventoryService;
        }

        public async Task<IReadOnlyList<SalesDeliveryListDto>> GetAllAsync(
            int? invoiceId = null,
            SalesDeliveryStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var deliveries = await _repository.GetAllByCompanyAsync(
                _currentUser.CompanyId, invoiceId, status, fromDate, toDate, cancellationToken);

            return deliveries.Select(d => new SalesDeliveryListDto
            {
                Id = d.Id,
                DeliveryNumber = d.DeliveryNumber,
                DeliveryDate = d.DeliveryDate,
                InvoiceNumber = d.SalesInvoice.InvoiceNumber,
                CustomerName = d.Customer.Name,
                WarehouseName = d.Warehouse.Name,
                Status = d.Status.ToString()
            }).ToList();
        }

        public async Task<SalesDeliveryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var delivery = await _repository.GetByIdWithLinesAsync(id, cancellationToken);

            if (delivery == null || delivery.CompanyId != _currentUser.CompanyId)
                return null;

            return MapToDto(delivery);
        }

        public async Task<SalesDeliveryDto> CreateAsync(CreateSalesDeliveryRequest request, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            // Validate invoice
            var invoice = await _invoiceRepository.GetByIdWithLinesAsync(request.SalesInvoiceId, cancellationToken)
                ?? throw BusinessErrors.InvoiceNotFound();

            if (invoice.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (invoice.Status == SalesInvoiceStatus.Draft)
                throw BusinessErrors.InvalidStatus("Cannot create delivery for a draft invoice.");

            if (invoice.Status == SalesInvoiceStatus.Cancelled)
                throw BusinessErrors.InvalidStatus("Cannot create delivery for a cancelled invoice.");

            // Validate warehouse
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId)
                ?? throw BusinessErrors.WarehouseNotFound();

            if (warehouse.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized("Warehouse does not belong to your company.");

            // Validate lines
            if (request.Lines == null || !request.Lines.Any())
                throw new BusinessException("NO_LINES", "Delivery must have at least one line.", 400);

            var delivery = new SalesDelivery
            {
                CompanyId = _currentUser.CompanyId,
                BranchId = request.BranchId,
                DeliveryNumber = await _repository.GenerateDeliveryNumberAsync(_currentUser.CompanyId, cancellationToken),
                DeliveryDate = request.DeliveryDate,
                SalesInvoiceId = request.SalesInvoiceId,
                CustomerId = invoice.CustomerId,
                WarehouseId = request.WarehouseId,
                Status = SalesDeliveryStatus.Draft,
                Notes = request.Notes?.Trim(),
                CreatedByUserId = _currentUser.UserId,
                Lines = new List<SalesDeliveryLine>()
            };

            foreach (var lineRequest in request.Lines)
            {
                var invoiceLine = invoice.Lines.FirstOrDefault(l => l.Id == lineRequest.SalesInvoiceLineId)
                    ?? throw new BusinessException("INVALID_LINE", $"Invoice line {lineRequest.SalesInvoiceLineId} not found.", 400);

                if (lineRequest.Quantity > invoiceLine.RemainingQuantity)
                    throw BusinessErrors.ExceedsRemainingQuantity();

                var line = new SalesDeliveryLine
                {
                    SalesInvoiceLineId = lineRequest.SalesInvoiceLineId,
                    ProductId = invoiceLine.ProductId,
                    UnitId = invoiceLine.UnitId,
                    Quantity = lineRequest.Quantity,
                    Notes = lineRequest.Notes?.Trim()
                };

                delivery.Lines.Add(line);
            }

            await _repository.AddAsync(delivery, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(await _repository.GetByIdWithLinesAsync(delivery.Id, cancellationToken)!);
        }

        public async Task<PostDeliveryResponse> PostAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            // ⚠️ IMPORTANT: Check if Inventory module is enabled before posting
            await _moduleAccess.EnsureInventoryEnabledAsync(cancellationToken);

            var delivery = await _repository.GetByIdWithLinesAsync(id, cancellationToken)
                ?? throw BusinessErrors.DeliveryNotFound();

            if (delivery.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (delivery.Status != SalesDeliveryStatus.Draft)
                throw BusinessErrors.InvalidStatus("Delivery can only be posted from Draft status.");

            // Create Stock Out request for Inventory
            var stockOutRequest = new StockOutRequest
            {
                BranchId = delivery.BranchId,
                DocDate = delivery.DeliveryDate,
                SourceType = "SalesDelivery",
                SourceId = delivery.Id,
                Notes = $"Sales Delivery: {delivery.DeliveryNumber}",
                Lines = delivery.Lines.Select(l => new StockOutLineRequest
                {
                    ProductId = l.ProductId,
                    WarehouseId = delivery.WarehouseId,
                    Quantity = l.Quantity,
                    UnitId = l.UnitId,
                    Notes = l.Notes
                }).ToList()
            };

            // Call Inventory Service to create stock out
            var inventoryResult = await _inventoryService.StockOutAsync(stockOutRequest, cancellationToken);

            // Update delivery status
            delivery.Status = SalesDeliveryStatus.Posted;
            delivery.InventoryDocumentId = inventoryResult.DocumentId;
            delivery.PostedByUserId = _currentUser.UserId;
            delivery.PostedAt = DateTime.UtcNow;
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedByUserId = _currentUser.UserId;

            // Update invoice line delivered quantities
            var invoice = await _invoiceRepository.GetByIdWithLinesAsync(delivery.SalesInvoiceId, cancellationToken)!;

            foreach (var deliveryLine in delivery.Lines)
            {
                var invoiceLine = invoice.Lines.First(l => l.Id == deliveryLine.SalesInvoiceLineId);
                invoiceLine.DeliveredQuantity += deliveryLine.Quantity;
            }

            // Update invoice status
            UpdateInvoiceDeliveryStatus(invoice);

            _repository.Update(delivery);
            _invoiceRepository.Update(invoice);
            await _repository.SaveChangesAsync(cancellationToken);

            return new PostDeliveryResponse
            {
                DeliveryId = delivery.Id,
                DeliveryNumber = delivery.DeliveryNumber,
                InventoryDocumentId = inventoryResult.DocumentId,
                InventoryDocNumber = inventoryResult.DocNumber
            };
        }

        public async Task<SalesDeliveryDto> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var delivery = await _repository.GetByIdWithLinesAsync(id, cancellationToken)
                ?? throw BusinessErrors.DeliveryNotFound();

            if (delivery.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (delivery.Status == SalesDeliveryStatus.Cancelled)
                throw BusinessErrors.InvalidStatus("Delivery is already cancelled.");

            if (delivery.Status == SalesDeliveryStatus.Posted)
                throw BusinessErrors.InvalidStatus("Cannot cancel a posted delivery. Create a sales return instead.");

            delivery.Status = SalesDeliveryStatus.Cancelled;
            delivery.UpdatedAt = DateTime.UtcNow;
            delivery.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(delivery);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(delivery);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var delivery = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw BusinessErrors.DeliveryNotFound();

            if (delivery.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (delivery.Status != SalesDeliveryStatus.Draft)
                throw BusinessErrors.InvalidStatus("Only draft deliveries can be deleted.");

            delivery.DeletedByUserId = _currentUser.UserId;
            _repository.Delete(delivery);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private void UpdateInvoiceDeliveryStatus(SalesInvoice invoice)
        {
            var allFullyDelivered = invoice.Lines.All(l => l.DeliveredQuantity >= l.Quantity);
            var anyDelivered = invoice.Lines.Any(l => l.DeliveredQuantity > 0);

            if (allFullyDelivered)
                invoice.Status = SalesInvoiceStatus.FullyDelivered;
            else if (anyDelivered)
                invoice.Status = SalesInvoiceStatus.PartiallyDelivered;
        }

        private SalesDeliveryDto MapToDto(SalesDelivery delivery) => new()
        {
            Id = delivery.Id,
            DeliveryNumber = delivery.DeliveryNumber,
            DeliveryDate = delivery.DeliveryDate,
            SalesInvoiceId = delivery.SalesInvoiceId,
            InvoiceNumber = delivery.SalesInvoice.InvoiceNumber,
            CustomerId = delivery.CustomerId,
            CustomerName = delivery.Customer.Name,
            WarehouseId = delivery.WarehouseId,
            WarehouseName = delivery.Warehouse.Name,
            Status = delivery.Status.ToString(),
            Notes = delivery.Notes,
            Lines = delivery.Lines.Select(l => new SalesDeliveryLineDto
            {
                Id = l.Id,
                SalesInvoiceLineId = l.SalesInvoiceLineId,
                ProductId = l.ProductId,
                ProductName = l.Product.Name,
                ProductCode = l.Product.Code,
                UnitId = l.UnitId,
                UnitName = l.Unit.Name,
                Quantity = l.Quantity
            }).ToList()
        };
    }
}