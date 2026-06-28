using ERPSystem.Application.Exceptions;
using ERPSystem.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Data
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;

        public UnitOfWork(AppDbContext db) => _db = db;

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
            catch (DbUpdateException ex)         
            {
                // Translate EF exception to a domain type before it escapes
                // Infrastructure. Application layer never sees DbUpdateException.
                await tx.RollbackAsync(ct);
                throw new DataConstraintException(
                    "A database constraint was violated. " +
                    "Check for duplicate values or invalid references.", ex);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}
