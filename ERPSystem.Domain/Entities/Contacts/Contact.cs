using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Abstractions;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Domain.Entities.Contacts
{
    public class Contact : AuditableEntity, ICompanyEntity
    {
        public required string FullName { get; set; }
        public int CompanyId { get; set; }
        public string Email { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public required string Company { get; set; }
        public string? Position { get; set; }
        public ContactPersonType Type { get; set; } // TODO: Replace enum ContactPersonType with ContactType lookup entity (seed system types + CRUD endpoints).
        public string? Location { get; set; }
        public string? Website { get; set; }
        public string? ProfileLink { get; set; }
        public string? Notes { get; set; }
        public bool Favorite { get; set; } = false;
    }
}