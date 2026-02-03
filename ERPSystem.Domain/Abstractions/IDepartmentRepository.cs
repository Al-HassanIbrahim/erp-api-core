using ERPSystem.Domain.Entities.HR;

namespace ERPSystem.Domain.Abstractions
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<Department?> GetByIdWithDetailsAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<IEnumerable<Department>> GetAllAsync(int companyId, CancellationToken ct = default);

        Task<bool> ExistsByCodeAsync(string code, int companyId, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, int companyId, CancellationToken ct = default);
        Task<int> GetEmployeeCountAsync(Guid departmentId, int companyId, CancellationToken ct = default);

        Task AddAsync(Department department, CancellationToken ct = default);
        Task UpdateAsync(Department department, CancellationToken ct = default);
        Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default);
    }
}
