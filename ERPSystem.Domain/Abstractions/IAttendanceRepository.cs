using ERPSystem.Domain.Entities.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface IAttendanceRepository
    {
        Task<Attendance?> GetByIdAsync(Guid id);
        Task<Attendance?> GetByEmployeeAndDateAsync(Guid employeeId, DateOnly date);
        Task<IEnumerable<Attendance>> GetByEmployeeAndPeriodAsync(Guid employeeId, DateOnly start, DateOnly end);
        Task<bool> HasCheckedInTodayAsync(Guid employeeId, DateOnly date);
        Task<bool> IsPayrollProcessedForPeriodAsync(Guid employeeId, DateOnly date);
        Task AddAsync(Attendance attendance);
        Task UpdateAsync(Attendance attendance);
        Task DeleteAsync(Guid id);
    }
}
