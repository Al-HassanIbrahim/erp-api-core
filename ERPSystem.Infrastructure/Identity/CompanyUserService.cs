using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Infrastructure.Identity
{
    public class CompanyUserService : ICompanyUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ICurrentUserService _currentUser;

        private static readonly string[] AllowedRoles = { "CompanyOwner", "CompanyAdmin", "CompanyUser" };

        public CompanyUserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUser = currentUser;
        }

        public async Task<IReadOnlyList<CompanyUserDto>> GetAllAsync(CancellationToken ct = default)
        {
            var users = await _userManager.Users
                .Where(u => u.CompanyId == _currentUser.CompanyId)
                .ToListAsync(ct);

            var result = new List<CompanyUserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new CompanyUserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                    IsLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    Roles = roles.ToList()
                });
            }
            return result;
        }

        public async Task<CompanyUserDto> CreateAsync(CreateCompanyUserDto dto, CancellationToken ct = default)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new BusinessException("EMAIL_EXISTS", "Email already exists.", 409);

            var rolesToAssign = dto.Roles?.Where(r => AllowedRoles.Contains(r)).ToList() ?? new List<string> { "CompanyUser" };

            foreach (var role in rolesToAssign)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                UserName = dto.Email.Trim().ToLowerInvariant(),
                FullName = dto.FullName?.Trim() ?? string.Empty,
                CompanyId = _currentUser.CompanyId,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BusinessException("CREATE_FAILED", string.Join(", ", result.Errors.Select(e => e.Description)), 400);

            await _userManager.AddToRolesAsync(user, rolesToAssign);

            return new CompanyUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = true,
                IsLockedOut = false,
                Roles = rolesToAssign
            };
        }

        public async Task<CompanyUserDto> UpdateRolesAsync(Guid userId, UpdateUserRolesDto dto, CancellationToken ct = default)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == _currentUser.CompanyId, ct)
                ?? throw new BusinessException("USER_NOT_FOUND", "User not found.", 404);

            var newRoles = dto.Roles.Where(r => AllowedRoles.Contains(r)).Distinct().ToList();
            if (!newRoles.Any())
                throw new BusinessException("INVALID_ROLES", "At least one valid role is required.", 400);

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRolesAsync(user, newRoles);

            return new CompanyUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                IsLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                Roles = newRoles
            };
        }

        public async Task<CompanyUserDto> UpdateStatusAsync(Guid userId, UpdateUserStatusDto dto, CancellationToken ct = default)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == _currentUser.CompanyId, ct)
                ?? throw new BusinessException("USER_NOT_FOUND", "User not found.", 404);

            if (user.Id == _currentUser.UserId && !dto.IsActive)
                throw new BusinessException("CANNOT_LOCK_SELF", "Cannot lock your own account.", 400);

            if (dto.IsActive)
                await _userManager.SetLockoutEndDateAsync(user, null);
            else
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new CompanyUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                IsActive = dto.IsActive,
                IsLockedOut = !dto.IsActive,
                Roles = roles.ToList()
            };
        }
    }
}