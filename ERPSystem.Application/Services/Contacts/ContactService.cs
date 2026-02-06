using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Application.DTOs.Contacts;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Entities.Contacts;

namespace ERPSystem.Application.Services.Contacts
{
    public class ContactService : IContactService
    {
        private readonly IContactRepository repo;
        private readonly IModuleAccessService moduleAccess;
        private readonly ICurrentUserService CurrentUser;

        public ContactService(IContactRepository repo,
            ICurrentUserService currentUser,
            IModuleAccessService moduleAccess)
        {
            this.repo = repo;
            CurrentUser = currentUser;
            this.moduleAccess = moduleAccess;
        }



        public async Task<List<ContactDetailsDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            await moduleAccess.EnsureContactEnabledAsync(cancellationToken);
            var contacts = await repo.GetAllContactsAsync(CurrentUser.CompanyId, cancellationToken);
            return contacts.Select(c => new ContactDetailsDto
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                Company = c.Company,
                Position = c.Position,
                Type = c.Type,
                Location = c.Location,
                PhoneNumber = c.PhoneNumber,
                Website = c.Website,
                ProfileLink = c.ProfileLink,
                Notes = c.Notes,
                Favorite = c.Favorite
            }).ToList();
        }

        public async Task<ContactDetailsDto?> GetContact(int Id, CancellationToken cancellationToken)
        {
            await moduleAccess.EnsureContactEnabledAsync(cancellationToken);
            var contact = await repo.GetByIdAsync(Id, CurrentUser.CompanyId, cancellationToken);
            if (contact == null)  BusinessErrors.ContactNotFound();
            return new ContactDetailsDto
            {
                Id = contact.Id,
                FullName = contact.FullName,
                Email = contact.Email,
                PhoneNumber = contact.PhoneNumber,
                Company = contact.Company,
                Position = contact.Position,
                Type = contact.Type,
                Location = contact.Location,
                Website = contact.Website,
                ProfileLink = contact.ProfileLink,
                Notes = contact.Notes,
                Favorite = contact.Favorite
            };
            
        }
        public async Task<ContactDto> Create(CreateContactRequest request, CancellationToken cancellationToken)
        {
            await moduleAccess.EnsureContactEnabledAsync(cancellationToken);
            var entity = new Contact
            {
                FullName = request.FullName,
                CompanyId=CurrentUser.CompanyId,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Company = request.Company,
                Position = request.Position,
                Type = request.Type,
                Location = request.Location,
                Website = request.Website,
                ProfileLink = request.ProfileLink,
                Notes = request.Notes,
                Favorite = request.Favorite
            };
            await repo.AddAsync(entity, cancellationToken);
            await repo.SaveChanges(cancellationToken);
            return new ContactDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                Company = entity.Company,
                Position = entity.Position,
                Type = entity.Type,
                Favorite = entity.Favorite
            };
        }

        public async Task<ContactDto> Update(UpdateContactDto request, CancellationToken cancellationToken)
        {
            await moduleAccess.EnsureContactEnabledAsync(cancellationToken);
            var entity = await repo.GetByIdAsync(request.Id, CurrentUser.CompanyId, cancellationToken);
            if (entity == null) BusinessErrors.ContactNotFound();

            entity.FullName = request.FullName;
            entity.Email = request.Email;
            entity.PhoneNumber = request.PhoneNumber;
            entity.Company = request.Company;
            entity.Position = request.Position;
            entity.Type = request.Type;
            entity.Location = request.Location;
            entity.Website = request.Website;
            entity.ProfileLink = request.ProfileLink;
            entity.Notes = request.Notes;
            entity.Favorite = request.Favorite;

            repo.Update(entity);
            await repo.SaveChanges(cancellationToken);

            return new ContactDto
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                Company = entity.Company,
                Position = entity.Position,
                Type = entity.Type,
                Favorite = entity.Favorite
            };
        }
        public async Task Delete(int id, CancellationToken cancellationToken)
        {
            await moduleAccess.EnsureContactEnabledAsync(cancellationToken);
            var entity = await repo.GetByIdAsync(id, CurrentUser.CompanyId, cancellationToken);
            if (entity == null) BusinessErrors.ContactNotFound();
            repo.Delete(entity);
            await repo.SaveChanges(cancellationToken);
        }
    }
}
