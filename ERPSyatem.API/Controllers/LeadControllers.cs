using ERPSystem.Application.DTOs.CRM;
using ERPSystem.Application.Interfaces;
using ERPSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERPSystem.API.Controllers.CRM
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeadController : ControllerBase
    {
        private readonly ILeadService _leadService;
        private readonly ICurrentUserService _currentUserService;

        public LeadController(ILeadService leadService, ICurrentUserService currentUserService)
        {
            _leadService = leadService;
            _currentUserService = currentUserService;
        }


        /// <summary>
        /// Get all leads with optional filtering:
        /// stage, source, assignedToId, search, converted
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<LeadDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<List<LeadDto>>> GetAll(CancellationToken ct = default)
        {
            try
            {
                var leads = await _leadService.ListAsync(ct: ct);

                return Ok(leads);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Get a lead by ID</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LeadDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<LeadDto>> GetById(int id, CancellationToken ct = default)
        {
            try
            {
                var lead = await _leadService.GetByIdAsync(id, ct);

                if (lead == null)
                    return NotFound(new { message = "Lead not found." });

                return Ok(lead);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        

        /// <summary>Create a new lead</summary>
        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> Create([FromBody] CreateLeadDto dto, CancellationToken ct = default)
        {
            try
            {
                var createdBy = _currentUserService.UserId.ToString();
                var leadId = await _leadService.CreateAsync(dto, createdBy, ct);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = leadId },
                    new { id = leadId, message = "Lead created successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Update an existing lead</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateLeadDto dto, CancellationToken ct = default)
        {
            try
            {
                await _leadService.UpdateAsync(id, dto, ct);

                return Ok(new { message = "Lead updated successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Delete a lead (only if not converted)</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
        {
            try
            {
                await _leadService.DeleteAsync(id, ct);
                return Ok(new { message = "Lead deleted successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Convert a lead to a customer and optionally create a deal</summary>
        [HttpPost("{id}/convert")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Convert(int id, [FromBody] ConvertLeadDto dto, CancellationToken ct = default)
        {
            try
            {
                var modifiedBy = _currentUserService.UserId.ToString();
                await _leadService.ConvertAsync(id, dto, modifiedBy, ct);

                return Ok(new
                {
                    message = "Lead converted successfully.",
                    customerId = dto.CustomerId,
                    dealCreated = dto.CreateDeal
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
