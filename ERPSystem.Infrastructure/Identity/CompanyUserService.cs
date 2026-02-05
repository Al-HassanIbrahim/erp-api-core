using ERPSystem.Application.Authorization;
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
                .Where(u => u.CompanyId == _currentUser.CompanyId && !u.IsDeleted)
                .ToListAsync(ct);

            var result = new List<CompanyUserDto>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                // Extract display names from scoped role names
                var displayRoles = roles
                    .Where(r => RoleKey.BelongsToCompany(r, _currentUser.CompanyId))
                    .Select(r => RoleKey.GetDisplayName(r))
                    .ToList();

                result.Add(new CompanyUserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                    IsLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    Roles = displayRoles
                });
            }
            return result;
        }

        public async Task<CompanyUserDto> CreateAsync(CreateCompanyUserDto dto, CancellationToken ct = default)
        {
            // Check email uniqueness (excluding soft-deleted users)
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email.Trim().ToLowerInvariant() && !u.IsDeleted, ct);

            if (existingUser != null)
                throw new BusinessException("EMAIL_EXISTS", "Email already exists.", 409);

            // Validate roles exist for company
            var rolesToAssign = new List<string>();
            if (dto.Roles != null && dto.Roles.Any())
            {
                foreach (var displayName in dto.Roles.Distinct())
                {
                    var trimmed = displayName?.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    var scopedName = RoleKey.ForCompany(_currentUser.CompanyId, trimmed);
                    var roleExists = await _roleManager.RoleExistsAsync(scopedName);
                    if (!roleExists)
                        throw BusinessErrors.RoleNotFound(trimmed);

                    rolesToAssign.Add(scopedName);
                }
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = dto.Email.Trim().ToLowerInvariant(),
                UserName = dto.Email.Trim().ToLowerInvariant(),
                FullName = dto.FullName?.Trim() ?? string.Empty,
                PhoneNumber = dto.PhoneNumber?.Trim(),
                CompanyId = _currentUser.CompanyId,
                EmailConfirmed = true,
                IsDeleted = false
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new BusinessException("CREATE_FAILED", FormatErrors(result.Errors), 400);

            // Assign validated roles
            if (rolesToAssign.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, rolesToAssign);
                if (!roleResult.Succeeded)
                    throw new BusinessException("ROLE_ASSIGN_FAILED", FormatErrors(roleResult.Errors), 400);
            }

            return new CompanyUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = true,
                IsLockedOut = false,
                Roles = rolesToAssign.Select(r => RoleKey.GetDisplayName(r)).ToList()
            };
        }

        public async Task<CompanyUserDto> UpdateStatusAsync(Guid userId, UpdateUserStatusDto dto, CancellationToken ct = default)
        {
            var user = await GetUserInCompanyAsync(userId, ct);

            // Prevent locking self
            if (user.Id == _currentUser.UserId && !dto.IsActive)
                throw new BusinessException("CANNOT_LOCK_SELF", "Cannot lock your own account.", 400);

            if (dto.IsActive)
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            else
            {
                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var displayRoles = roles
                .Where(r => RoleKey.BelongsToCompany(r, _currentUser.CompanyId))
                .Select(r => RoleKey.GetDisplayName(r))
                .ToList();

            return new CompanyUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = dto.IsActive,
                IsLockedOut = !dto.IsActive,
                Roles = displayRoles
            };
        }

        public async Task<CompanyUserDto> AssignRoleAsync(Guid userId, string roleDisplayName, CancellationToken ct = default)
        {
            var user = await GetUserInCompanyAsync(userId, ct);

            var trimmed = roleDisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw BusinessErrors.RoleNameRequired();

            var scopedName = RoleKey.ForCompany(_currentUser.CompanyId, trimmed);

            // Validate role exists for this company
            if (!await _roleManager.RoleExistsAsync(scopedName))
                throw BusinessErrors.RoleNotFound(trimmed);

            // Check if already assigned
            if (await _userManager.IsInRoleAsync(user, scopedName))
            {
                // Already has role, just return current state
                return await BuildUserDtoAsync(user);
            }

            var result = await _userManager.AddToRoleAsync(user, scopedName);
            if (!result.Succeeded)
                throw new BusinessException("ROLE_ASSIGN_FAILED", FormatErrors(result.Errors), 400);

            return await BuildUserDtoAsync(user);
        }

        public async Task<CompanyUserDto> RemoveRoleAsync(Guid userId, string roleDisplayName, CancellationToken ct = default)
        {
            var user = await GetUserInCompanyAsync(userId, ct);

            var trimmed = roleDisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                throw BusinessErrors.RoleNameRequired();

            var scopedName = RoleKey.ForCompany(_currentUser.CompanyId, trimmed);

            // Validate role exists for this company
            if (!await _roleManager.RoleExistsAsync(scopedName))
                throw BusinessErrors.RoleNotFound(trimmed);

            // Check if user has this role
            if (!await _userManager.IsInRoleAsync(user, scopedName))
            {
                // User doesn't have this role, just return current state
                return await BuildUserDtoAsync(user);
            }

            var result = await _userManager.RemoveFromRoleAsync(user, scopedName);
            if (!result.Succeeded)
                throw new BusinessException("ROLE_REMOVE_FAILED", FormatErrors(result.Errors), 400);

            return await BuildUserDtoAsync(user);
        }

        public async Task<CompanyUserDto> UpdateProfileAsync(Guid userId, AdminUpdateUserProfileDto dto, CancellationToken ct = default)
        {
            var user = await GetUserInCompanyAsync(userId, ct);

            // Update FullName
            user.FullName = dto.FullName?.Trim() ?? user.FullName;

            // Update PhoneNumber (admin-managed)
            user.PhoneNumber = dto.PhoneNumber?.Trim();

            // Optionally update Email if provided and different
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
                if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    // Check uniqueness (excluding soft-deleted users)
                    var existingUser = await _userManager.Users
                        .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted && u.Id != user.Id);

                    if (existingUser != null)
                        throw new BusinessException("EMAIL_EXISTS", "Email already exists.", 409);

                    user.Email = normalizedEmail;
                    user.UserName = normalizedEmail;
                    user.NormalizedEmail = normalizedEmail.ToUpperInvariant();
                    user.NormalizedUserName = normalizedEmail.ToUpperInvariant();
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BusinessException("UPDATE_FAILED", FormatErrors(result.Errors), 400);

            return await BuildUserDtoAsync(user);
        }

        public async Task DeleteAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await GetUserInCompanyAsync(userId, ct);

            // Prevent deleting self
            if (user.Id == _currentUser.UserId)
                throw new BusinessException("CANNOT_DELETE_SELF", "Cannot delete your own account.", 400);

            // Soft delete
            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.DeletedByUserId = _currentUser.UserId;

            // Also lock the account to prevent login
            await _userManager.SetLockoutEnabledAsync(user, true);
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new BusinessException("DELETE_FAILED", FormatErrors(result.Errors), 400);
        }

        #region Private Helpers

        private async Task<ApplicationUser> GetUserInCompanyAsync(Guid userId, CancellationToken ct)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == _currentUser.CompanyId && !u.IsDeleted, ct);

            if (user == null)
                throw BusinessErrors.UserNotFound();

            return user;
        }

        private async Task<CompanyUserDto> BuildUserDtoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var displayRoles = roles
                .Where(r => RoleKey.BelongsToCompany(r, _currentUser.CompanyId))
                .Select(r => RoleKey.GetDisplayName(r))
                .ToList();

            return new CompanyUserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow,
                IsLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                Roles = displayRoles
            };
        }

        private static string FormatErrors(IEnumerable<IdentityError> errors)
        {
            return string.Join(", ", errors.Select(e => e.Description));
        }

        #endregion
    }
}