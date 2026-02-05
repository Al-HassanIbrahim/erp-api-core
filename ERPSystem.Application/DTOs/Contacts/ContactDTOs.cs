using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERPSystem.Domain.Enums;

namespace ERPSystem.Application.DTOs.Contacts
{
    public record ContactDto
    {
        public int Id { get; init; }
        public string FullName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string Company { get; init; } = default!;
        public string? Position { get; init; }
        public ContactPersonType Type { get; set; }
        public bool Favorite { get; init; }
    }
    public record ContactDetailsDto
    {
        public int Id { get; init; }
        public string FullName { get; init; } = default!;
        public string Email { get; init; } = default!;
        public string? PhoneNumber { get; init; }
        public string Company { get; init; } = default!;
        public string? Position { get; init; }
        public ContactPersonType Type { get; set; }
        public string? Location { get; init; }
        public string? Website { get; init; }
        public string? ProfileLink { get; init; }
        public string? Notes { get; init; }
        public bool Favorite { get; init; }
    }
    public record CreateContactRequest
    {
        [Required]
        public string FullName { get; init; } = default!;
        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;
        [Phone]
        public string? PhoneNumber { get; init; }
        [Required]
        public string Company { get; init; } = default!;
        public string? Position { get; init; }
        [Required]
        public ContactPersonType Type { get; set; }
        public string? Location { get; init; }
        public string? Website { get; init; }
        public string? ProfileLink { get; init; }
        public string? Notes { get; init; }
        public bool Favorite { get; init; } = false;
    }
    public record UpdateContactDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string FullName { get; init; } = default!;
        [Required]
        [EmailAddress]
        public string Email { get; init; } = default!;
        [Phone]
        public string? PhoneNumber { get; init; }
        [Required]
        public string Company { get; init; } = default!;
        public string? Position { get; init; }
        [Required]
        public ContactPersonType Type { get; set; }
        public string? Location { get; init; }
        public string? Website { get; init; }
        public string? ProfileLink { get; init; }
        public string? Notes { get; init; }
        public bool Favorite { get; init; } = false;
    }

}
