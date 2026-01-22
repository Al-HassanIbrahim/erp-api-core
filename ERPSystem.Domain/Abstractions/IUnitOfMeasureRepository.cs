using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Domain.Abstractions
{
    public interface IUnitOfMeasureRepository
    {
        Task<UnitOfMeasure?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<UnitOfMeasure>> GetAllByCompanyAsync(int companyId, CancellationToken cancellationToken = default);
        Task AddAsync(UnitOfMeasure unitOfMeasure, CancellationToken cancellationToken = default);
        void Update(UnitOfMeasure unitOfMeasure);
        void Delete(UnitOfMeasure unitOfMeasure);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
