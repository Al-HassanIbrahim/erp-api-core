using ERPSystem.Domain.Abstractions;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Repositories.Purchase
{
    public sealed class PurchasingUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;

        public PurchasingUnitOfWork(AppDbContext db) => _db = db;

        /// <inheritdoc/>
        public async Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> action,
            CancellationToken ct = default)
        {
            // If a transaction is already active (e.g. ambient caller) reuse it.
            // This prevents nested BEGIN TRANSACTION errors in SQL Server.
            if (_db.Database.CurrentTransaction is not null)
            {
                await action(ct);
                return;
            }

            await using var tx = await _db.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.ReadCommitted, ct);
            try
            {
                await action(ct);
                await tx.CommitAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Roll back first, then re-throw so the service converts this
                // to a domain ConflictException (HTTP 409).
                await tx.RollbackAsync(ct);
                throw;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}
