using ERPSystem.Domain.Entities.HR;

namespace ERPSystem.Domain.Abstractions
{
    public interface IAttendanceRepository
    {
        Task<Attendance?> GetByIdAsync(Guid id, int companyId, CancellationToken ct = default);
        Task<Attendance?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date, int companyId, CancellationToken ct = default);
        Task<IReadOnlyList<Attendance>> GetByEmployeeAndPeriodAsync(Guid employeeId, DateOnly start, DateOnly end, int companyId, CancellationToken ct = default);

        Task<bool> HasCheckedInTodayAsync(Guid employeeId, DateOnly date, int companyId, CancellationToken ct = default);
        Task<bool> IsPayrollProcessedForPeriodAsync(Guid employeeId, DateOnly date, int companyId, CancellationToken ct = default);

        Task AddAsync(Attendance attendance);
        Task UpdateAsync(Attendance attendance);

        Task DeleteAsync(Guid id, int companyId, CancellationToken ct = default);
    }
}
