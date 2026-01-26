using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Services.Sales
{
    public class SalesInvoiceService : ISalesInvoiceService
    {
        private readonly ISalesInvoiceRepository _repository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfMeasureRepository _unitRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        public SalesInvoiceService(
            ISalesInvoiceRepository repository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IUnitOfMeasureRepository unitRepository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _repository = repository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _unitRepository = unitRepository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        public async Task<IReadOnlyList<SalesInvoiceListDto>> GetAllAsync(
            int? customerId = null,
            SalesInvoiceStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var invoices = await _repository.GetAllByCompanyAsync(
                _currentUser.CompanyId, customerId, status, fromDate, toDate, cancellationToken);

            return invoices.Select(i => new SalesInvoiceListDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                InvoiceDate = i.InvoiceDate,
                CustomerName = i.Customer.Name,
                Status = i.Status.ToString(),
                PaymentStatus = i.PaymentStatus.ToString(),
                GrandTotal = i.GrandTotal,
                BalanceDue = i.BalanceDue
            }).ToList();
        }

        public async Task<SalesInvoiceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var invoice = await _repository.GetByIdWithLinesAsync(id, cancellationToken);

            if (invoice == null || invoice.CompanyId != _currentUser.CompanyId)
                return null;

            return MapToDto(invoice);
        }

        public async Task<SalesInvoiceDto> CreateAsync(CreateSalesInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            // Validate customer
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
                ?? throw BusinessErrors.CustomerNotFound();

            if (customer.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized("Customer does not belong to your company.");

            // Validate lines
            if (request.Lines == null || !request.Lines.Any())
                throw new BusinessException("NO_LINES", "Invoice must have at least one line.", 400);

            var invoice = new SalesInvoice
            {
                CompanyId = _currentUser.CompanyId,
                BranchId = request.BranchId,
                InvoiceNumber = await _repository.GenerateInvoiceNumberAsync(_currentUser.CompanyId, cancellationToken),
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                CustomerId = request.CustomerId,
                Status = SalesInvoiceStatus.Draft,
                PaymentStatus = PaymentStatus.Unpaid,
                Notes = request.Notes?.Trim(),
                CreatedByUserId = _currentUser.UserId,
                Lines = new List<SalesInvoiceLine>()
            };

            foreach (var lineRequest in request.Lines)
            {
                var product = await _productRepository.GetByIdAsync(lineRequest.ProductId)
                    ?? throw BusinessErrors.ProductNotFound();

                if (product.CompanyId != _currentUser.CompanyId)
                    throw BusinessErrors.Unauthorized("Product does not belong to your company.");

                var line = CreateInvoiceLine(lineRequest, product.Id);
                invoice.Lines.Add(line);
            }

            CalculateInvoiceTotals(invoice);

            await _repository.AddAsync(invoice, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(await _repository.GetByIdWithLinesAsync(invoice.Id, cancellationToken)!);
        }

        public async Task<SalesInvoiceDto> UpdateAsync(int id, UpdateSalesInvoiceRequest request, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var invoice = await _repository.GetByIdWithLinesAsync(id, cancellationToken)
                ?? throw BusinessErrors.InvoiceNotFound();

            if (invoice.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (invoice.Status != SalesInvoiceStatus.Draft)
                throw BusinessErrors.CannotModifyPostedDocument();

            // Validate customer
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
                ?? throw BusinessErrors.CustomerNotFound();

            if (customer.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized("Customer does not belong to your company.");

            invoice.InvoiceDate = request.InvoiceDate;
            invoice.DueDate = request.DueDate;
            invoice.CustomerId = request.CustomerId;
            invoice.Notes = request.Notes?.Trim();
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedByUserId = _currentUser.UserId;

            // Clear existing lines and recreate
            invoice.Lines.Clear();

            foreach (var lineRequest in request.Lines)
            {
                var product = await _productRepository.GetByIdAsync(lineRequest.ProductId)
                    ?? throw BusinessErrors.ProductNotFound();

                if (product.CompanyId != _currentUser.CompanyId)
                    throw BusinessErrors.Unauthorized("Product does not belong to your company.");

                var line = CreateInvoiceLine(new CreateSalesInvoiceLineRequest
                {
                    ProductId = lineRequest.ProductId,
                    UnitId = lineRequest.UnitId,
                    Quantity = lineRequest.Quantity,
                    UnitPrice = lineRequest.UnitPrice,
                    DiscountPercent = lineRequest.DiscountPercent,
                    TaxPercent = lineRequest.TaxPercent,
                    Notes = lineRequest.Notes
                }, product.Id);

                invoice.Lines.Add(line);
            }

            CalculateInvoiceTotals(invoice);

            _repository.Update(invoice);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(await _repository.GetByIdWithLinesAsync(invoice.Id, cancellationToken)!);
        }

        public async Task<SalesInvoiceDto> PostAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var invoice = await _repository.GetByIdWithLinesAsync(id, cancellationToken)
                ?? throw BusinessErrors.InvoiceNotFound();

            if (invoice.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (invoice.Status != SalesInvoiceStatus.Draft)
                throw BusinessErrors.InvalidStatus("Invoice can only be posted from Draft status.");

            invoice.Status = SalesInvoiceStatus.Posted;
            invoice.PostedByUserId = _currentUser.UserId;
            invoice.PostedAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(invoice);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(invoice);
        }

        public async Task<SalesInvoiceDto> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var invoice = await _repository.GetByIdWithLinesAsync(id, cancellationToken)
                ?? throw BusinessErrors.InvoiceNotFound();

            if (invoice.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (invoice.Status == SalesInvoiceStatus.Cancelled)
                throw BusinessErrors.InvalidStatus("Invoice is already cancelled.");

            // Check if there are deliveries - cannot cancel if deliveries exist
            if (invoice.Status == SalesInvoiceStatus.PartiallyDelivered || invoice.Status == SalesInvoiceStatus.FullyDelivered)
                throw BusinessErrors.InvalidStatus("Cannot cancel invoice with deliveries. Cancel deliveries first.");

            invoice.Status = SalesInvoiceStatus.Cancelled;
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(invoice);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(invoice);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var invoice = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw BusinessErrors.InvoiceNotFound();

            if (invoice.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (invoice.Status != SalesInvoiceStatus.Draft)
                throw BusinessErrors.InvalidStatus("Only draft invoices can be deleted.");

            invoice.DeletedByUserId = _currentUser.UserId;
            _repository.Delete(invoice);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private SalesInvoiceLine CreateInvoiceLine(CreateSalesInvoiceLineRequest request, int productId)
        {
            var lineSubTotal = request.Quantity * request.UnitPrice;
            var discountAmount = lineSubTotal * (request.DiscountPercent / 100);
            var taxableAmount = lineSubTotal - discountAmount;
            var taxAmount = taxableAmount * (request.TaxPercent / 100);
            var lineTotal = taxableAmount + taxAmount;

            return new SalesInvoiceLine
            {
                ProductId = productId,
                UnitId = request.UnitId,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                DiscountPercent = request.DiscountPercent,
                DiscountAmount = discountAmount,
                TaxPercent = request.TaxPercent,
                TaxAmount = taxAmount,
                LineTotal = lineTotal,
                DeliveredQuantity = 0,
                Notes = request.Notes?.Trim()
            };
        }

        private void CalculateInvoiceTotals(SalesInvoice invoice)
        {
            invoice.SubTotal = invoice.Lines.Sum(l => l.Quantity * l.UnitPrice);
            invoice.DiscountAmount = invoice.Lines.Sum(l => l.DiscountAmount);
            invoice.TaxAmount = invoice.Lines.Sum(l => l.TaxAmount);
            invoice.GrandTotal = invoice.Lines.Sum(l => l.LineTotal);
        }

        private SalesInvoiceDto MapToDto(SalesInvoice invoice) => new()
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            DueDate = invoice.DueDate,
            CustomerId = invoice.CustomerId,
            CustomerName = invoice.Customer.Name,
            Status = invoice.Status.ToString(),
            PaymentStatus = invoice.PaymentStatus.ToString(),
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            GrandTotal = invoice.GrandTotal,
            PaidAmount = invoice.PaidAmount,
            BalanceDue = invoice.BalanceDue,
            Notes = invoice.Notes,
            Lines = invoice.Lines.Select(l => new SalesInvoiceLineDto
            {
                Id = l.Id,
                ProductId = l.ProductId,
                ProductName = l.Product.Name,
                ProductCode = l.Product.Code,
                UnitId = l.UnitId,
                UnitName = l.Unit.Name,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountPercent = l.DiscountPercent,
                DiscountAmount = l.DiscountAmount,
                TaxPercent = l.TaxPercent,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                DeliveredQuantity = l.DeliveredQuantity,
                RemainingQuantity = l.RemainingQuantity
            }).ToList()
        };
    }
}