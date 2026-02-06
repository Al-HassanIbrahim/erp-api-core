using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Application.Services.Core
{
    public class CompanyProfileService : ICompanyProfileService
    {
        private readonly ICompanyRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public CompanyProfileService(ICompanyRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<CompanyMeDto?> GetMyCompanyAsync(CancellationToken ct = default)
        {
            var company = await _repository.GetByIdAsync(_currentUser.CompanyId, ct);
            if (company == null) return null;

            return new CompanyMeDto
            {
                Id = company.Id,
                Name = company.Name,
                CommercialName = company.CommercialName,
                TaxNumber = company.TaxNumber,
                Phone = company.Phone,
                Address = company.Address,
                IsActive = company.IsActive,
                CreatedAt = company.CreatedAt
            };
        }

        public async Task<CompanyMeDto> UpdateMyCompanyAsync(UpdateCompanyMeDto dto, CancellationToken ct = default)
        {
            var company = await _repository.GetByIdTrackingAsync(_currentUser.CompanyId, ct)
                ?? throw new BusinessException("COMPANY_NOT_FOUND", "Company not found.", 404);

            company.Name = dto.Name.Trim();
            company.CommercialName = dto.CommercialName?.Trim();
            company.TaxNumber = dto.TaxNumber.Trim();
            company.Phone = dto.Phone?.Trim();
            company.Address = dto.Address?.Trim();
            company.UpdatedAt = DateTime.UtcNow;
            company.UpdatedByUserId = _currentUser.UserId;

            _repository.Update(company);
            await _repository.SaveChangesAsync(ct);

            return new CompanyMeDto
            {
                Id = company.Id,
                Name = company.Name,
                CommercialName = company.CommercialName,
                Phone = company.Phone,
                Address = company.Address,
                TaxNumber = company.TaxNumber,
                IsActive = company.IsActive,
                CreatedAt = company.CreatedAt
            };
        }
    }
}