using ERPSystem.Application.DTOs.Core;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Public: create company + owner user + return token
        [HttpPost("register-owner")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AuthResponse>> RegisterOwner(
            [FromBody] RegisterOwnerRequest request,
            CancellationToken ct)
        {
            var result = await _authService.RegisterOwnerAsync(request, ct);
            return Ok(result);
        }

        // Public: login for any user (owner/admin/employee)
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<AuthResponse>> Login(
            [FromBody] LoginRequest request,
            CancellationToken ct)
        {
            var result = await _authService.LoginAsync(request, ct);
            return Ok(result);
        }
    }
}
