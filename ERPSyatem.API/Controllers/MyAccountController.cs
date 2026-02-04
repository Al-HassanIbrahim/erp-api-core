using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    /// <summary>
    /// Self-service endpoints for the current logged-in user.
    /// </summary>
    [ApiController]
    [Route("api/my-account")]
    [Authorize]
    public class MyAccountController : ControllerBase
    {
        private readonly IMyAccountService _service;

        public MyAccountController(IMyAccountService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets the current user's profile.
        /// PhoneNumber is read-only (managed by admin).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(MyAccountDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<MyAccountDto>> GetProfile(CancellationToken ct)
        {
            var result = await _service.GetProfileAsync(ct);
            return Ok(result);
        }

        /// <summary>
        /// Changes the current user's password.
        /// </summary>
        [HttpPut("password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            await _service.ChangePasswordAsync(request, ct);
            return NoContent();
        }
        // TODO
        /// <summary>
        /// Uploads a profile image for the current user.
        /// Allowed formats: jpg, jpeg, png, webp. Max size: 2MB.
        /// </summary>
        //[HttpPost("profile-image")]
        //[ProducesResponseType(typeof(ProfileImageUploadResult), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<ProfileImageUploadResult>> UploadProfileImage(IFormFile file, CancellationToken ct)
        //{
        //    var result = await _service.UploadProfileImageAsync(file, ct);
        //    return Ok(result);
        //}
    }
}