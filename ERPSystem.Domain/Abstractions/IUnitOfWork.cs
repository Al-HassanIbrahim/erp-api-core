using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Domain.Abstractions
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Execute <paramref | name ="action"/> inside a single atomic
        /// database transaction.  Any exception causes a full rollback;
        /// DbUpdateConcurrencyException is re-thrown as-is so the
        /// service can convert it to a domain ConflictException.
        /// </summary>
        Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken ct = default);
    }
}
