using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Products;

namespace ERPSystem.Domain.Abstractions
{
    public interface IUnitOfMeasureRepository
    {
        Task<UnitOfMeasure?> GetByIdAsync(int id);
        Task<IEnumerable<UnitOfMeasure>> GetAllAsync();
        Task AddAsync(UnitOfMeasure unitOfMeasure);
        Task UpdateAsync(UnitOfMeasure unitOfMeasure);
        Task DeleteAsync(int id);
    }
}
