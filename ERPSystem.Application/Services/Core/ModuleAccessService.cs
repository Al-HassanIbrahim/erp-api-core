using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;

namespace ERPSystem.Application.Services.Core
{
    public class ModuleAccessService : IModuleAccessService
    {
        private readonly ICompanyModuleRepository _companyModuleRepository;
        private readonly ICurrentUserService _currentUser;

        private const string SalesModuleCode = "SALES";
        private const string InventoryModuleCode = "INVENTORY";
        private const string HrModuleCode = "HR";
        private const string ContactModuleCode = "CONTACT";
        private const string ExpensesModuleCode = "EXPENSES";

        public ModuleAccessService(
            ICompanyModuleRepository companyModuleRepository,
            ICurrentUserService currentUser)
        {
            _companyModuleRepository = companyModuleRepository;
            _currentUser = currentUser;
        }

        public async Task<bool> IsModuleEnabledAsync(int companyId, string moduleCode, CancellationToken cancellationToken = default)
        {
            return await _companyModuleRepository.IsModuleEnabledAsync(companyId, moduleCode, cancellationToken);
        }

        public async Task<bool> IsSalesEnabledAsync(CancellationToken cancellationToken = default)
        {
            return await IsModuleEnabledAsync(_currentUser.CompanyId, SalesModuleCode, cancellationToken);
        }

        public async Task<bool> IsInventoryEnabledAsync(CancellationToken cancellationToken = default)
        {
            return await IsModuleEnabledAsync(_currentUser.CompanyId, InventoryModuleCode, cancellationToken);
        }
        public Task<bool> IsHrEnabledAsync(CancellationToken ct = default) => IsModuleEnabledAsync(_currentUser.CompanyId, HrModuleCode, ct);
        public async Task<bool> IsContactEnabledAsync(CancellationToken cancellationToken = default)
        {
            return await IsModuleEnabledAsync(_currentUser.CompanyId, ContactModuleCode, cancellationToken);
        }
        public async Task<bool> IsExpensesEnabledAsync(CancellationToken cancellationToken = default)
        {
            return await IsModuleEnabledAsync(_currentUser.CompanyId, ExpensesModuleCode, cancellationToken);
        }

        public async Task EnsureSalesEnabledAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsSalesEnabledAsync(cancellationToken))
                throw BusinessErrors.SalesModuleNotEnabled();
        }

        public async Task EnsureInventoryEnabledAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsInventoryEnabledAsync(cancellationToken))
                throw BusinessErrors.InventoryModuleNotEnabled();
        }

        public async Task EnsureHrEnabledAsync(CancellationToken ct = default) 
        {
            if (!await IsHrEnabledAsync(ct))
                throw BusinessErrors.HrModuleNotEnabled(); 
        public async Task EnsureContactEnabledAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsContactEnabledAsync(cancellationToken))
                throw BusinessErrors.ContactModuleNotEnabled();
        }
        public async Task EnsureExpensesEnabledAsync(CancellationToken cancellationToken = default)
        {
            if (!await IsExpensesEnabledAsync(cancellationToken))
                throw BusinessErrors.ExpensesModuleNotEnabled();
        }
    }
}