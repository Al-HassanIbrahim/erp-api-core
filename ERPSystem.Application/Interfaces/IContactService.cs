using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Contacts;

namespace ERPSystem.Application.Interfaces
{
    public interface IContactService
    {
        Task<List<ContactDetailsDto>> GetAllAsync(CancellationToken cancellationToken);
        Task<ContactDetailsDto?> GetContact(int Id,CancellationToken cancellationToken);
        Task<ContactDto> Create(CreateContactRequest request,CancellationToken cancellationToken);
        Task<ContactDto> Update(UpdateContactDto request,CancellationToken cancellationToken);
        Task Delete(int id,CancellationToken cancellationToken);

    }
}
