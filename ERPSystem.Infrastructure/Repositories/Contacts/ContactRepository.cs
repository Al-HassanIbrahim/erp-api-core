using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Contacts;
using ERPSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Repositories.Contacts
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext context;

        public ContactRepository(AppDbContext context)
        {
            this.context = context;
        }


        async Task<List<Contact>> IContactRepository.GetAllContactsAsync(int companyId, CancellationToken cancellationToken)
        {
            return await context.Contacts.AsNoTracking().Where(c => c.CompanyId== companyId && !c.IsDeleted)
                .ToListAsync(cancellationToken);

        }

        public Task<Contact?> GetByIdAsync(int Id, int companyId, CancellationToken cancellationToken)
        {
            return context.Contacts.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == Id && !c.IsDeleted, cancellationToken);
        }

        public async Task AddAsync(Contact contact, CancellationToken cancellationToken)
        {
            await context.Contacts.AddAsync(contact, cancellationToken);
        }
        public void Update(Contact contact)
        {
            context.Contacts.Update(contact);
        }

        public void Delete(Contact contact)
        {
            contact.IsDeleted = true;
            Update(contact);
        }

        public async Task SaveChanges(CancellationToken cancellationToken)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

    }
}
