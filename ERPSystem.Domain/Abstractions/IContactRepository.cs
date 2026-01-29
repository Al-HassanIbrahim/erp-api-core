using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Entities.Contacts;

namespace ERPSystem.Domain.Abstractions
{
    public interface IContactRepository
    {
        Task<List<Contact>> GetAllContactsAsync(int companyId,CancellationToken cancellationToken);
        Task<Contact?> GetByIdAsync(int Id,int companyId,CancellationToken cancellationToken);
        Task AddAsync(Contact contact,CancellationToken cancellationToken);
        void Update(Contact contact);
        void Delete(Contact contact);
        Task SaveChanges(CancellationToken cancellationToke);
    }
}
