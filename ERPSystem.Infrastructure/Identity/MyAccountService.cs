using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Exceptions;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ERPSystem.Infrastructure.Identity
{
    public class MyAccountService : IMyAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;

        public MyAccountService(
            UserManager<ApplicationUser> userManager,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<MyAccountDto> GetProfileAsync(CancellationToken ct = default)
        {
            var user = await GetCurrentUserAsync();

            return new MyAccountDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                CompanyId = user.CompanyId,
                ProfileImageUrl = user.ProfileImageUrl,
                PhoneNumber = user.PhoneNumber // Read-only
            };
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
        {
            var user = await GetCurrentUserAsync();

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("PASSWORD_CHANGE_FAILED", errors, 400);
            }
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var user = await _userManager.FindByIdAsync(_currentUser.UserId.ToString());
            if (user == null)
                throw BusinessErrors.UserNotFound();

            // Double-check company isolation
            if (user.CompanyId != _currentUser.CompanyId)
                throw BusinessErrors.Unauthorized();

            return user;
        }

    }
}