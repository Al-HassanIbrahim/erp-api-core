using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPSystem.Application.DTOs.Core
{
    // Request DTO for registering an owner along with their company details
    public record RegisterOwnerRequest(
        string FullName,
        string Email,
        string Password,
        CompanyOnboardingRequest Company
    );

    public record CompanyOnboardingRequest(
        string Name,
        string? TaxNumber,
        string? CommercialName,
        string? Address,
        [Phone] string? PhoneNumber
    );

    //--------------------------------------------------------

    public record LoginRequest(
        string Email,
        string Password
    );

    public record AuthResponse(
        string AccessToken,
        DateTime ExpiresAtUtc,
        Guid UserId,
        int CompanyId,
        string Email,
        string[] Roles,
        string[] Permissions
    );
}
