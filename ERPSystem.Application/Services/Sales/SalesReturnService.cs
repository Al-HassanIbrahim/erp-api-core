using ERPSystem.Application.DTOs.Inventory;
using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Services.Sales
{
    public class SalesReturnService : ISalesReturnService
    {
        private readonly ISalesReturnRepository _repository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;
        private readonly IInventoryService _inventoryService;

        public SalesReturnService(
            ISalesReturnRepository repository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IWarehouseRepository warehouseRepository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess,
            IInventoryService inventoryService)
        {
            _repository = repository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
            _inventoryService = inventoryService;
        }

        public async Task<IReadOnlyList<SalesReturnListDto>> GetAllAsync(
            int? customerId = null,
            SalesReturnStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var returns = await _repository.GetAllByCompanyAsync(
                _currentUser.CompanyId, customerId, status, fromDate, toDate, cancellationToken);

            return returns.Select(r => new SalesReturnListDto
            {
                Id = r.Id,
                ReturnNumber = r.ReturnNumber,
                ReturnDate = r.ReturnDate,
                CustomerName = r.Customer.Name,
                WarehouseName = r.Warehouse.Name,
                Status = r.Status.ToString(),
                GrandTotal = r.GrandTotal
            }).ToList();
        }

        public async Task<SalesReturnDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var salesReturn = await _repository.GetByIdWithLinesAsync(id, cancellationToken);

            if (salesReturn == null || salesReturn.CompanyId != _currentUser.CompanyId)
                return null;

            return MapToDto(salesReturn);
        }

        public async Task<SalesReturnDto> CreateAsync(CreateSalesReturnRequest request, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            // Validate customer
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
                ?? throw BusinessErrors.CustomerNotFound();

            if (customer.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized("Customer does not belong to your company.");

            // Validate warehouse
            var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId)
                ?? throw BusinessErrors.WarehouseNotFound();

            if (warehouse.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized("Warehouse does not belong to your company.");

            // Validate lines
            if (request.Lines == null || !request.Lines.Any())
                throw new BusinessException("NO_LINES", "Return must have at least one line.", 400);

            var salesReturn = new SalesReturn
            {
                CompanyId = _currentUser.CompanyId,
                BranchId = request.BranchId,
                ReturnNumber = await _repository.GenerateReturnNumberAsync(_currentUser.CompanyId, cancellationToken),
                ReturnDate = request.ReturnDate,
                SalesInvoiceId = request.SalesInvoiceId,
                CustomerId = request.CustomerId,
                WarehouseId = request.WarehouseId,
                Status = SalesReturnStatus.Draft,
                Reason = request.Reason?.Trim(),
                Notes = request.Notes?.Trim(),
                CreatedByUserId = _currentUser.UserId,
                Lines = new List<SalesReturnLine>()
            };

            foreach (var lineRequest in request.Lines)
            {
                var product = await _productRepository.GetByIdAsync(lineRequest.ProductId)
                    ?? throw BusinessErrors.ProductNotFound();

                if (product.CompanyId != _currentUser.CompanyId)
                    throw BusinessErrors.Unauthorized("Product does not belong to your company.");

                var line = CreateReturnLine(lineRequest);
                salesReturn.Lines.Add(line);
            }

            CalculateReturnTotals(salesReturn);

            await _repository.AddAsync(salesReturn, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(await _repository.GetByIdWithLinesAsync(salesReturn.Id, cancellationToken)!);
        }

        public async Task<PostReturnResponse> PostAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            // ⚠️ IMPORTANT: Check if Inventory module is enabled before posting
            await _moduleAccess.EnsureInventoryEnabledAsync(cancellationToken);

            var salesReturn = await _repository.GetByIdWithLinesAsync(id, cancellationToken)
                ?? throw BusinessErrors.ReturnNotFound();

            if (salesReturn.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (salesReturn.Status != SalesReturnStatus.Draft)
                throw BusinessErrors.InvalidStatus("Return can only be posted from Draft status.");

            // Create Stock In request for Inventory
            var stockInRequest = new StockInRequest
            {
                BranchId = salesReturn.BranchId,
                DocDate = salesReturn.ReturnDate,
                SourceType = "SalesReturn",
                SourceId = salesReturn.Id,
                Notes = $"Sales Return: {salesReturn.ReturnNumber}",
                Lines = salesReturn.Lines.Select(l => new StockInLineRequest
                {
                    ProductId = l.ProductId,
                    WarehouseId = salesReturn.WarehouseId,
                    Quantity = l.Quantity,
                    UnitId = l.UnitId,
                    UnitCost = l.UnitPrice, // Use return price as cost
                    Notes = l.Notes
                }).ToList()
            };

            // Call Inventory Service to create stock in
            var inventoryResult = await _inventoryService.StockInAsync(stockInRequest, cancellationToken);

            // Update return status
            salesReturn.Status = SalesReturnStatus.Posted;
            salesReturn.InventoryDocumentId = inventoryResult.DocumentId;
            salesReturn.PostedByUserId = _currentUser.UserId;
            salesReturn.PostedAt = DateTime.UtcNow;
            salesReturn.UpdatedAt = DateTime.UtcNow;
            salesReturn.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(salesReturn);
            await _repository.SaveChangesAsync(cancellationToken);

            return new PostReturnResponse
            {
                ReturnId = salesReturn.Id,
                ReturnNumber = salesReturn.ReturnNumber,
                InventoryDocumentId = inventoryResult.DocumentId,
                InventoryDocNumber = inventoryResult.DocNumber
            };
        }

        public async Task<SalesReturnDto> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var salesReturn = await _repository.GetByIdWithLinesAsync(id, cancellationToken)
                ?? throw BusinessErrors.ReturnNotFound();

            if (salesReturn.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (salesReturn.Status == SalesReturnStatus.Cancelled)
                throw BusinessErrors.InvalidStatus("Return is already cancelled.");

            if (salesReturn.Status == SalesReturnStatus.Posted)
                throw BusinessErrors.InvalidStatus("Cannot cancel a posted return.");

            salesReturn.Status = SalesReturnStatus.Cancelled;
            salesReturn.UpdatedAt = DateTime.UtcNow;
            salesReturn.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(salesReturn);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(salesReturn);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var salesReturn = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw BusinessErrors.ReturnNotFound();

            if (salesReturn.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (salesReturn.Status != SalesReturnStatus.Draft)
                throw BusinessErrors.InvalidStatus("Only draft returns can be deleted.");

            salesReturn.DeletedByUserId = _currentUser.UserId;
            _repository.Delete(salesReturn);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private SalesReturnLine CreateReturnLine(CreateSalesReturnLineRequest request)
        {
            var lineSubTotal = request.Quantity * request.UnitPrice;
            var taxAmount = lineSubTotal * (request.TaxPercent / 100);
            var lineTotal = lineSubTotal + taxAmount;

            return new SalesReturnLine
            {
                ProductId = request.ProductId,
                UnitId = request.UnitId,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                TaxPercent = request.TaxPercent,
                TaxAmount = taxAmount,
                LineTotal = lineTotal,
                Notes = request.Notes?.Trim()
            };
        }

        private void CalculateReturnTotals(SalesReturn salesReturn)
        {
            salesReturn.SubTotal = salesReturn.Lines.Sum(l => l.Quantity * l.UnitPrice);
            salesReturn.TaxAmount = salesReturn.Lines.Sum(l => l.TaxAmount);
            salesReturn.GrandTotal = salesReturn.Lines.Sum(l => l.LineTotal);
        }

        private SalesReturnDto MapToDto(SalesReturn salesReturn) => new()
        {
            Id = salesReturn.Id,
            ReturnNumber = salesReturn.ReturnNumber,
            ReturnDate = salesReturn.ReturnDate,
            SalesInvoiceId = salesReturn.SalesInvoiceId,
            InvoiceNumber = salesReturn.SalesInvoice?.InvoiceNumber,
            CustomerId = salesReturn.CustomerId,
            CustomerName = salesReturn.Customer.Name,
            WarehouseId = salesReturn.WarehouseId,
            WarehouseName = salesReturn.Warehouse.Name,
            Status = salesReturn.Status.ToString(),
            SubTotal = salesReturn.SubTotal,
            TaxAmount = salesReturn.TaxAmount,
            GrandTotal = salesReturn.GrandTotal,
            Reason = salesReturn.Reason,
            Notes = salesReturn.Notes,
            Lines = salesReturn.Lines.Select(l => new SalesReturnLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = l.Product.Name,
                ProductCode = l.Product.Code,
                UnitId = l.UnitId,
                UnitName = l.Unit.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                TaxPercent = l.TaxPercent,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal
            }).ToList()
        };
    }
}