using ERPSystem.Application.DTOs.Contacts;
using ERPSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ERPSyatem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _contactService.GetAllAsync(cancellationToken);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContact(int id, CancellationToken cancellationToken)
        {
            var result = await _contactService.GetContact(id, cancellationToken);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateContactRequest request, CancellationToken cancellationToken)
        {
            var result = _contactService.Create(request, cancellationToken);
            return CreatedAtAction(nameof(GetContact), new { id = result.Id }, result);
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateContactDto request, CancellationToken cancellationToken)
        {
            var result = _contactService.Update(request, cancellationToken);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAAsync(int id, CancellationToken cancellationToken)
        {
            var result = _contactService.Delete(id, cancellationToken);
            return NoContent();
        }
    }
}
