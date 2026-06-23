using ERPSystem.Application.Interfaces;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Infrastructure.Shared
{
    public sealed class DocumentSequenceService(AppDbContext db) : IDocumentSequenceService
    {
        public async Task<string> GenerateNextNumberAsync(
            int companyId, string documentType, string prefix, CancellationToken ct = default)
        {
            var yearMonth = DateTime.UtcNow.ToString("yyyyMM");

            // Strict safety: this method must be called within an active transaction
            // to guarantee sequence consistency and prevent lost numbers.
            if (db.Database.CurrentTransaction is null)
                throw new InvalidOperationException(
                    "GenerateNextNumberAsync must be called inside an active database transaction.");

            var conn = db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            await using var cmd = conn.CreateCommand();
            cmd.Transaction = db.Database.CurrentTransaction.GetDbTransaction();

            // Atomic UPSERT on System_DocumentSequences with HOLDLOCK
            // to ensure safe sequence generation under concurrent requests.
            cmd.CommandText = """
            MERGE System_DocumentSequences WITH (HOLDLOCK) AS T
            USING (VALUES (@CompanyId, @DocumentType, @YearMonth))
                  AS S(CompanyId, DocumentType, YearMonth)
               ON T.CompanyId    = S.CompanyId
              AND T.DocumentType = S.DocumentType
              AND T.YearMonth    = S.YearMonth
            WHEN MATCHED THEN
                UPDATE SET T.LastSequence = T.LastSequence + 1
            WHEN NOT MATCHED THEN
                INSERT (CompanyId, DocumentType, YearMonth, LastSequence)
                VALUES (@CompanyId, @DocumentType, @YearMonth, 1)
            OUTPUT INSERTED.LastSequence;
            """;

            var pCompanyId = cmd.CreateParameter();
            pCompanyId.ParameterName = "@CompanyId";
            pCompanyId.Value = companyId;
            cmd.Parameters.Add(pCompanyId);

            var pDocType = cmd.CreateParameter();
            pDocType.ParameterName = "@DocumentType";
            pDocType.Value = documentType;
            cmd.Parameters.Add(pDocType);

            var pYearMonth = cmd.CreateParameter();
            pYearMonth.ParameterName = "@YearMonth";
            pYearMonth.Value = yearMonth;
            cmd.Parameters.Add(pYearMonth);

            var seq = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));

            // Return the document number using a company- and month-based format
            return $"{prefix}-{yearMonth}-C{companyId}-{seq:D5}";
        }
    }
}
