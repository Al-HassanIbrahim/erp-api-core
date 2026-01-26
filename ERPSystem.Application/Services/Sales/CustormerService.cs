using ERPSystem.Application.DTOs.Sales;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Sales;

namespace ERPSystem.Application.Services.Sales
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly IModuleAccessService _moduleAccess;

        public CustomerService(
            ICustomerRepository repository,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            _repository = repository;
            _currentUser = currentUser;
            _moduleAccess = moduleAccess;
        }

        public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(bool? isActive = null, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var customers = await _repository.GetAllByCompanyAsync(_currentUser.CompanyId, isActive, cancellationToken);

            return customers.Select(MapToDto).ToList();
        }

        public async Task<CustomerDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var customer = await _repository.GetByIdAsync(id, cancellationToken);

            if (customer == null || customer.CompanyId != _currentUser.CompanyId)
                return null;

            return MapToDto(customer);
        }

        public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            // Check if code already exists
            if (await _repository.ExistsAsync(_currentUser.CompanyId, request.Code.Trim(), null, cancellationToken))
                throw new BusinessException("DUPLICATE_CODE", "Customer code already exists.", 400);

            var customer = new Customer
            {
                CompanyId = _currentUser.CompanyId,
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Email = request.Email?.Trim(),
                Phone = request.Phone?.Trim(),
                Address = request.Address?.Trim(),
                TaxNumber = request.TaxNumber?.Trim(),
                CreditLimit = request.CreditLimit,
                IsActive = true,
                CreatedByUserId = _currentUser.UserId
            };

            await _repository.AddAsync(customer, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(customer);
        }

        public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var customer = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw BusinessErrors.CustomerNotFound();

            if (customer.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            customer.Name = request.Name.Trim();
            customer.Email = request.Email?.Trim();
            customer.Phone = request.Phone?.Trim();
            customer.Address = request.Address?.Trim();
            customer.TaxNumber = request.TaxNumber?.Trim();
            customer.CreditLimit = request.CreditLimit;
            customer.IsActive = request.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(customer);
            await _repository.SaveChangesAsync(cancellationToken);

            return MapToDto(customer);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _moduleAccess.EnsureSalesEnabledAsync(cancellationToken);

            var customer = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw BusinessErrors.CustomerNotFound();

            if (customer.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            customer.DeletedByUserId = _currentUser.UserId;
            _repository.Delete(customer);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        private static CustomerDto MapToDto(Customer customer) => new()
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            TaxNumber = customer.TaxNumber,
            CreditLimit = customer.CreditLimit,
            IsActive = customer.IsActive
        };
    }
}