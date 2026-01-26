using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.Services.Sales
{
    public class SalesReceiptService : ISalesReceiptService
    {
        private readonly ISalesReceiptRepository _repository;
        private readonly ISalesInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        public SalesReceiptService(
            ISalesReceiptRepository repository,
            ISalesInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _repository = repository;
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        public async Task<IReadOnlyList<SalesReceiptListDto>> GetAllAsync(
            int? customerId = null,
            SalesReceiptStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var receipts = await _repository.GetAllByCompanyAsync(
                _currentUser.CompanyId, customerId, status, fromDate, toDate, cancellationToken);

            return receipts.Select(r => new SalesReceiptListDto
            {
                Id = r.Id,
                ReceiptNumber = r.ReceiptNumber,
                ReceiptDate = r.ReceiptDate,
                CustomerName = r.Customer.Name,
                Amount = r.Amount,
                Status = r.Status.ToString()
            }).ToList();
        }

        public async Task<SalesReceiptDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var receipt = await _repository.GetByIdWithAllocationsAsync(id, cancellationToken);

            if (receipt == null || receipt.CompanyId != _currentUser.CompanyId)
                return null;

            return MapToDto(receipt);
        }

        public async Task<SalesReceiptDto> CreateAsync(CreateSalesReceiptRequest request, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            // Validate customer
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken)
                ?? throw BusinessErrors.CustomerNotFound();

            if (customer.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized("Customer does not belong to your company.");

            // Validate total allocation matches amount
            var totalAllocated = request.Allocations?.Sum(a => a.AllocatedAmount) ?? 0;
            if (totalAllocated > request.Amount)
                throw new BusinessException("OVER_ALLOCATION", "Total allocation exceeds receipt amount.", 400);

            var receipt = new SalesReceipt
            {
                CompanyId = _currentUser.CompanyId,
                BranchId = request.BranchId,
                ReceiptNumber = await _repository.GenerateReceiptNumberAsync(_currentUser.CompanyId, cancellationToken),
                ReceiptDate = request.ReceiptDate,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod?.Trim(),
                ReferenceNumber = request.ReferenceNumber?.Trim(),
                Status = SalesReceiptStatus.Draft,
                Notes = request.Notes?.Trim(),
                CreatedByUserId = _currentUser.UserId,
                Allocations = new List<SalesReceiptAllocation>()
            };

            // Add allocations
            if (request.Allocations != null)
            {
                foreach (var allocationRequest in request.Allocations)
                {
                    var invoice = await _invoiceRepository.GetByIdAsync(allocationRequest.SalesInvoiceId, cancellationToken)
                        ?? throw BusinessErrors.InvoiceNotFound();

                    if (invoice.CompanyId != _currentUser.CompanyId)
                        throw BusinessErrors.Unauthorized("Invoice does not belong to your company.");

                    if (invoice.CustomerId != request.CustomerId)
                        throw new BusinessException("WRONG_CUSTOMER", "Invoice does not belong to the selected customer.", 400);

                    if (allocationRequest.AllocatedAmount > invoice.BalanceDue)
                        throw BusinessErrors.AllocationExceedsBalance();

                    receipt.Allocations.Add(new SalesReceiptAllocation
                    {
                        SalesInvoiceId = allocationRequest.SalesInvoiceId,
                        AllocatedAmount = allocationRequest.AllocatedAmount
                    });
                }
            }

            await _repository.AddAsync(receipt, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(await _repository.GetByIdWithAllocationsAsync(receipt.Id, cancellationToken)!);
        }

        public async Task<SalesReceiptDto> PostAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var receipt = await _repository.GetByIdWithAllocationsAsync(id, cancellationToken)
                ?? throw BusinessErrors.ReceiptNotFound();

            if (receipt.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (receipt.Status != SalesReceiptStatus.Draft)
                throw BusinessErrors.InvalidStatus("Receipt can only be posted from Draft status.");

            // Apply allocations to invoices
            foreach (var allocation in receipt.Allocations)
            {
                var invoice = await _invoiceRepository.GetByIdAsync(allocation.SalesInvoiceId, cancellationToken)!;

                invoice.PaidAmount += allocation.AllocatedAmount;

                // Update payment status
                if (invoice.PaidAmount >= invoice.GrandTotal)
                    invoice.PaymentStatus = PaymentStatus.Paid;
                else if (invoice.PaidAmount > 0)
                    invoice.PaymentStatus = PaymentStatus.PartiallyPaid;

                _invoiceRepository.Update(invoice);
            }

            receipt.Status = SalesReceiptStatus.Posted;
            receipt.PostedByUserId = _currentUser.UserId;
            receipt.PostedAt = DateTime.UtcNow;
            receipt.UpdatedAt = DateTime.UtcNow;
            receipt.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(receipt);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(receipt);
        }

        public async Task<SalesReceiptDto> CancelAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var receipt = await _repository.GetByIdWithAllocationsAsync(id, cancellationToken)
                ?? throw BusinessErrors.ReceiptNotFound();

            if (receipt.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (receipt.Status == SalesReceiptStatus.Cancelled)
                throw BusinessErrors.InvalidStatus("Receipt is already cancelled.");

            // If posted, reverse the allocations
            if (receipt.Status == SalesReceiptStatus.Posted)
            {
                foreach (var allocation in receipt.Allocations)
                {
                    var invoice = await _invoiceRepository.GetByIdAsync(allocation.SalesInvoiceId, cancellationToken)!;

                    invoice.PaidAmount -= allocation.AllocatedAmount;

                    // Update payment status
                    if (invoice.PaidAmount <= 0)
                    {
                        invoice.PaidAmount = 0;
                        invoice.PaymentStatus = PaymentStatus.Unpaid;
                    }
                    else
                        invoice.PaymentStatus = PaymentStatus.PartiallyPaid;

                    _invoiceRepository.Update(invoice);
                }
            }

            receipt.Status = SalesReceiptStatus.Cancelled;
            receipt.UpdatedAt = DateTime.UtcNow;
            receipt.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(receipt);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(receipt);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var receipt = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw BusinessErrors.ReceiptNotFound();

            if (receipt.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            if (receipt.Status != SalesReceiptStatus.Draft)
                throw BusinessErrors.InvalidStatus("Only draft receipts can be deleted.");

            receipt.DeletedByUserId = _currentUser.UserId;
            _repository.Delete(receipt);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private SalesReceiptDto MapToDto(SalesReceipt receipt) => new()
        {
            Id = receipt.Id,
            ReceiptNumber = receipt.ReceiptNumber,
            ReceiptDate = receipt.ReceiptDate,
            CustomerId = receipt.CustomerId,
            CustomerName = receipt.Customer.Name,
            Amount = receipt.Amount,
            PaymentMethod = receipt.PaymentMethod,
            ReferenceNumber = receipt.ReferenceNumber,
            Status = receipt.Status.ToString(),
            Notes = receipt.Notes,
            Allocations = receipt.Allocations.Select(a => new SalesReceiptAllocationDto
            {
                Id = a.Id,
                SalesInvoiceId = a.SalesInvoiceId,
                InvoiceNumber = a.SalesInvoice.InvoiceNumber,
                AllocatedAmount = a.AllocatedAmount
            }).ToList()
        };
    }
}